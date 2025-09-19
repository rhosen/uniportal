using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using UniPortal.Dtos;
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
        public async Task<List<StudentOnboardDto>> GetStudentsWithoutStudentIdAsync()
        {
            var studentRoleName = Roles.Student;

            var query = from account in _unitOfWork.Context.Accounts
                        join user in _unitOfWork.Context.Users
                            on account.IdentityId equals user.Id
                        join userRole in _unitOfWork.Context.UserRoles
                            on user.Id equals userRole.UserId
                        join role in _unitOfWork.Context.Roles
                            on userRole.RoleId equals role.Id
                        where role.Name == studentRoleName
                              && account.IsActive && !account.IsDeleted
                              && !_unitOfWork.Context.Students
                                  .Any(s => s.AccountId == account.Id && !string.IsNullOrEmpty(s.StudentId))
                        select new StudentOnboardDto
                        {
                            AccountId = account.Id,
                            Email = account.Email,
                            CreatedAt = account.CreatedAt
                        };

            return await query.AsNoTracking().ToListAsync();
        }


        // Create or update student record
        public async Task CreateOrUpdateStudentAsync(Student student)
        {
            if (student == null)
                throw new ArgumentNullException(nameof(student));

            try
            {
                // Check if student already exists
                var existingStudent = await _unitOfWork.Context.Students
                    .FirstOrDefaultAsync(s => s.AccountId == student.AccountId);

                if (existingStudent != null)
                {
                    existingStudent.StudentId = student.StudentId?.Trim();
                    existingStudent.BatchNumber = student.BatchNumber?.Trim();
                    existingStudent.Section = student.Section?.Trim();
                    existingStudent.ProgramId = student.ProgramId;
                    existingStudent.CurrentSemesterId = student.CurrentSemesterId;
                    existingStudent.UpdatedAt = DateTime.Now;

                    await LogAsync(existingStudent.AccountId, ActionType.Update, "Student", existingStudent.Id, student);
                }
                else
                {
                    student.Id = Guid.NewGuid();
                    student.CreatedAt = DateTime.Now;
                    await _unitOfWork.Context.Students.AddAsync(student);
                    await LogAsync(student.AccountId, ActionType.Create, "Student", student.Id, student);
                }

                // Commit changes
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
            var query = from s in _unitOfWork.Context.Students
                        join a in _unitOfWork.Context.Accounts on s.AccountId equals a.Id
                        join p in _unitOfWork.Context.Programs on s.ProgramId equals p.Id
                        join d in _unitOfWork.Context.Departments on p.DepartmentId equals d.Id
                        join sem in _unitOfWork.Context.Semesters
                            on s.CurrentSemesterId equals sem.Id into semJoin
                        from sem in semJoin.DefaultIfEmpty() // LEFT JOIN
                        where !s.IsDeleted && a.IsActive && !a.IsDeleted
                        select new StudentViewModel
                        {
                            Id = s.Id,
                            AccountId = s.AccountId,
                            StudentId = s.StudentId,
                            BatchNumber = s.BatchNumber,
                            Section = s.Section,
                            ProgramId = s.ProgramId,
                            DepartmentId = d.Id,
                            CurrentSemesterId = s.CurrentSemesterId,
                            Email = a.Email,
                            ProgramName = p.Name,
                            DepartmentCode = d.Code,
                            CurrentSemesterName = sem != null ? sem.Name : null // Handle null
                        };

            return await query
                .OrderBy(s => s.StudentId)
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<List<StudentDto>> GetAllActiveStudentAsync()
        {
            var query = from s in _context.Students
                        join a in _context.Accounts on s.AccountId equals a.Id
                        join p in _context.Programs on s.ProgramId equals p.Id
                        join d in _context.Departments on p.DepartmentId equals d.Id
                        where !s.IsDeleted && a.IsActive
                        select new StudentDto
                        {
                            Id = s.Id, // <-- real ID for saving
                            StudentId = s.StudentId, // numeric ID (can be human-readable if needed)
                            FullName = a.FirstName + " " + a.LastName,
                            DepartmentName = d.Name,
                            ProgramName = p.Name,
                            CurrentSemesterId = s.CurrentSemesterId
                        };

            return await query.ToListAsync();
        }



        public async Task<Student> GetStudentAsync(
        Guid? accountId = null,
        Guid? studentId = null,
        string studentCode = null) // student.StudentId
        {
            if (accountId == null && studentId == null && string.IsNullOrEmpty(studentCode))
                throw new ArgumentException("At least one identifier must be provided.");

            var query = _context.Students
                .AsNoTracking()
                .Include(s => s.Account)       // for profile info
                .Include(s => s.Program)       // include Program to get Department if needed
                    .ThenInclude(p => p.Department)
                .AsQueryable();

            if (accountId.HasValue)
                query = query.Where(s => s.AccountId == accountId.Value);

            if (studentId.HasValue)
                query = query.Where(s => s.Id == studentId.Value);

            if (!string.IsNullOrEmpty(studentCode))
                query = query.Where(s => s.StudentId == studentCode);

            query = query.Where(s => !s.IsDeleted && s.Account.IsActive);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<StudentDto> GetStudentByIdAsync(Guid studentId)
        {
            var query = from s in _context.Students
                        join a in _context.Accounts on s.AccountId equals a.Id
                        join p in _context.Programs on s.ProgramId equals p.Id
                        join d in _context.Departments on p.DepartmentId equals d.Id
                        join sem in _context.Semesters on s.CurrentSemesterId equals sem.Id into semJoin
                        from sem in semJoin.DefaultIfEmpty() // in case CurrentSemesterId is null
                        where !s.IsDeleted && a.IsActive && s.Id == studentId
                        select new StudentDto
                        {
                            Id = s.Id,
                            StudentId = s.StudentId,
                            FullName = a.FirstName + " " + a.LastName,
                            ProgramName = p.Name,
                            DepartmentName = d.Name,
                            CurrentSemesterId = s.CurrentSemesterId,
                            CurrentSemesterName = sem != null ? sem.Name : null
                        };

            return await query.FirstOrDefaultAsync();
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

        public async Task<bool> DeleteAsync(Guid accountId)
        {
            // Delegate soft delete to AccountService
            await _accountService.SoftDeleteAsync(accountId);

            // Log deletion for student entity
            var student = await _unitOfWork.Context.Students
                .FirstOrDefaultAsync(s => s.AccountId == accountId);

            if (student != null)
                await LogAsync(accountId, ActionType.Delete, "Student", student.Id);

            return true;
        }
    }
}
