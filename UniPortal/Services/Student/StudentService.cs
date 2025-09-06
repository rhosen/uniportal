using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities;

namespace UniPortal.Services.Student
{
    public class StudentService
    {
        private readonly UniPortalContext _context;
        private readonly Random _random = new();

        public StudentService(UniPortalContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all students who do not have a StudentId assigned yet.
        /// </summary>
        public async Task<List<Account>> GetStudentsWithoutStudentIdAsync()
        {
            var studentRoleName = Roles.Student;

            var query = from account in _context.Accounts
                        join user in _context.Users
                            on account.IdentityUserId equals user.Id
                        join userRole in _context.UserRoles
                            on user.Id equals userRole.UserId
                        join role in _context.Roles
                            on userRole.RoleId equals role.Id
                        where role.Name == studentRoleName
                              && account.IsActive
                              && !_context.Students
                                  .Any(s => s.AccountId == account.Id && !string.IsNullOrEmpty(s.StudentId))
                        select account;

            return await query.ToListAsync();
        }

        /// <summary>
        /// Generates and assigns a unique student ID for a given account.
        /// If student entry exists, updates; otherwise, inserts a new record.
        /// </summary>
        public async Task<string> AssignStudentIdAsync(Guid accountId)
        {
            if (accountId == Guid.Empty)
                throw new ArgumentException("Invalid account ID");

            // Get the account's admission year from CreatedAt
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == accountId);

            if (account == null)
                throw new Exception("Account not found");

            int admissionYear = account.CreatedAt.Year;

            // Generate unique student ID
            string studentId = await GenerateUniqueStudentIdAsync(admissionYear);

            // Insert or update student record
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.AccountId == accountId);

            if (student != null)
            {
                student.StudentId = studentId;
                student.UpdatedAt = DateTime.Now;
            }
            else
            {
                student = new Data.Entities.Student
                {
                    Id = Guid.NewGuid(),
                    AccountId = accountId,
                    StudentId = studentId,
                    CreatedAt = DateTime.Now
                };
                _context.Students.Add(student);
            }

            await _context.SaveChangesAsync();
            return studentId;
        }

        /// <summary>
        /// Generates a unique student ID based on year, sequential number, and random suffix.
        /// Ensures no collisions in the database.
        /// </summary>
        private async Task<string> GenerateUniqueStudentIdAsync(int admissionYear)
        {
            string studentId;
            int maxAttempts = 10;
            int attempt = 0;

            do
            {
                attempt++;
                string sequentialNumber = await GetNextSequentialNumberAsync(admissionYear);
                string suffix = GenerateRandomSuffix(2);

                studentId = $"Y{admissionYear % 100:D2}{sequentialNumber}{suffix}";

            } while (await _context.Students.AnyAsync(s => s.StudentId == studentId) && attempt < maxAttempts);

            if (attempt >= maxAttempts)
                throw new Exception("Unable to generate unique Student ID after multiple attempts.");

            return studentId;
        }

        private async Task<string> GetNextSequentialNumberAsync(int admissionYear)
        {
            var existingIds = await _context.Students
                .Where(s => s.StudentId.StartsWith($"Y{admissionYear % 100:D2}"))
                .Select(s => s.StudentId)
                .ToListAsync();

            int nextNumber = 1;
            if (existingIds.Any())
            {
                var numbers = existingIds
                    .Select(id =>
                    {
                        if (id.Length >= 6 && int.TryParse(id.Substring(3, 4), out var num))
                            return num;
                        return 0;
                    }).ToList();

                nextNumber = numbers.Max() + 1;
            }

            return nextNumber.ToString("D4");
        }

        private string GenerateRandomSuffix(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Range(0, length)
                .Select(_ => chars[_random.Next(chars.Length)]).ToArray());
        }
    }
}
