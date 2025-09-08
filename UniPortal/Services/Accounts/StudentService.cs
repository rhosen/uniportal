using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Helpers;
using UniPortal.Services.Infrastructures;
using UniPortal.ViewModels.Users;

namespace UniPortal.Services.Accounts
{
    public class StudentService : BaseService<Student>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly StudentIdGenerator _idGenerator;
        private readonly AccountService _accountService;

        public StudentService(
            IUnitOfWork unitOfWork,
            LogService logService,
            StudentIdGenerator idGenerator,
            AccountService accountService)
            : base(unitOfWork.Context, logService)
        {
            _unitOfWork = unitOfWork;
            _idGenerator = idGenerator;
            _accountService = accountService;
        }

        // Get accounts of active students without StudentId
        public async Task<List<Account>> GetStudentsWithoutStudentIdAsync()
        {
            var studentRoleName = Roles.Student;

            var query = from account in _unitOfWork.Context.Accounts
                        join user in _unitOfWork.Context.Users
                            on account.IdentityUserId equals user.Id
                        join userRole in _unitOfWork.Context.UserRoles
                            on user.Id equals userRole.UserId
                        join role in _unitOfWork.Context.Roles
                            on userRole.RoleId equals role.Id
                        where role.Name == studentRoleName
                              && account.IsActive && !account.IsDeleted
                              && !_unitOfWork.Context.Students
                                  .Any(s => s.AccountId == account.Id && !string.IsNullOrEmpty(s.StudentId))
                        select account;

            return await query.AsNoTracking().ToListAsync();
        }

        // Create or update student record
        public async Task CreateOrUpdateStudentAsync(Student student)
        {
            if (student == null) throw new ArgumentNullException(nameof(student));

            try
            {
                var existingStudent = await _unitOfWork.Context.Students
                    .FirstOrDefaultAsync(s => s.AccountId == student.AccountId);

                if (existingStudent != null)
                {
                    existingStudent.StudentId = student.StudentId;
                    existingStudent.BatchNumber = student.BatchNumber;
                    existingStudent.Section = student.Section;
                    existingStudent.DepartmentId = student.DepartmentId;
                    existingStudent.UpdatedAt = DateTime.UtcNow;

                    _unitOfWork.Context.Students.Update(existingStudent);
                    await LogAsync(existingStudent.AccountId, ActionType.Update, "Student", existingStudent.Id, student);
                }
                else
                {
                    student.Id = Guid.NewGuid();
                    student.CreatedAt = DateTime.UtcNow;

                    await _unitOfWork.Context.Students.AddAsync(student);
                    await LogAsync(student.AccountId, ActionType.Create, "Student", student.Id, student);
                }

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        // Get all onboarded students
        public async Task<List<StudentViewModel>> GetAllOnboardedStudentsAsync()
        {
            return await _unitOfWork.Context.Students
                .Where(s => !s.IsDeleted && s.Account.IsActive)
                .Include(s => s.Account)
                .Select(s => new StudentViewModel
                {
                    Id = s.Id,
                    StudentId = s.StudentId,
                    BatchNumber = s.BatchNumber,
                    Section = s.Section,
                    DepartmentId = s.DepartmentId,
                    Email = s.Account.Email,
                    AccountId = s.AccountId
                })
                .OrderBy(s => s.StudentId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Student> GetStudentAsync(
            Guid? accountId = null,
            Guid? studentId = null,
            string studentCode = null) // student.StudentId
        {
            if (accountId == null && studentId == null && string.IsNullOrEmpty(studentCode))
                throw new ArgumentException("At least one identifier must be provided.");

            var query = _context.Students
                .Include(s => s.Account)       // for profile info
                .Include(s => s.Department)    // for department name
                .AsQueryable();

            if (accountId.HasValue)
                query = query.Where(s => s.AccountId == accountId.Value);

            if (studentId.HasValue)
                query = query.Where(s => s.Id == studentId.Value);

            if (!string.IsNullOrEmpty(studentCode))
                query = query.Where(s => s.StudentId == studentCode);

            query = query.Where(s => !s.IsDeleted && s.Account.IsActive);

            return await query.AsNoTracking().FirstOrDefaultAsync();
        }


        // Generate a unique StudentId
        public async Task<string> GetSystemGeneratedStudentId(Guid accountId)
        {
            if (accountId == Guid.Empty)
                throw new ArgumentException("Invalid account ID");

            var account = await _unitOfWork.Context.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == accountId);

            if (account == null)
                throw new Exception("Account not found");

            int admissionYear = account.CreatedAt.Year;

            // Provide all existing IDs to generator
            var existingIds = await _unitOfWork.Context.Students
                .Where(s => s.StudentId.StartsWith($"Y{admissionYear % 100:D2}"))
                .Select(s => s.StudentId)
                .ToListAsync();

            return _idGenerator.GenerateStudentId(admissionYear, existingIds);
        }

        // Soft-delete a student (delegates account deletion)
        public async Task<bool> DeleteAsync(Guid accountId)
        {
            var account = await _unitOfWork.Context.Accounts
                .FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted);

            if (account == null) return false;

            // Delegate soft delete to AccountService
            await _accountService.SoftDeleteAsync(accountId);

            // Log deletion for student entity
            var student = await _unitOfWork.Context.Students
                .FirstOrDefaultAsync(s => s.AccountId == accountId);

            if (student != null)
                await LogAsync(accountId, ActionType.Delete, "Student", student.Id);

            // Commit all changes via UnitOfWork
            await _unitOfWork.CommitAsync();
            return true;
        }
    }
}
