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

        public async Task<int> GetCurrentSemesterNumber(Guid studentId)
        {
            var student = await _context.Students
          .Where(s => s.Id == studentId && !s.IsDeleted)
          .Select(s => new { s.Id, s.CurrentSemester })
          .FirstOrDefaultAsync();

            if (student == null)
                throw new InvalidOperationException("Student not found.");

            int semesterNumber = student.CurrentSemester;
            return semesterNumber;
        }

        // Get accounts of active students without StudentNumber
        public async Task<List<StudentOnboardingDto>> GetStudentsWithoutStudentIdAsync()
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
                                  .Any(s => s.AccountId == account.Id && !string.IsNullOrEmpty(s.StudentNumber))
                        select new StudentOnboardingDto
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
                var existingStudent = await _unitOfWork.Context.Students
                    .FirstOrDefaultAsync(s => s.AccountId == student.AccountId);

                if (existingStudent != null)
                {
                    existingStudent.StudentNumber = student.StudentNumber?.Trim();
                    existingStudent.BatchId = student.BatchId;
                    existingStudent.SectionId = student.SectionId;
                    existingStudent.ProgramId = student.ProgramId;
                    existingStudent.CurrentSemester = student.CurrentSemester;
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
                        join b in _unitOfWork.Context.Batches on s.BatchId equals b.Id
                        join sec in _unitOfWork.Context.Sections on s.SectionId equals sec.Id
                        where !s.IsDeleted && a.IsActive && !a.IsDeleted
                        select new StudentViewModel
                        {
                            Id = s.Id,
                            AccountId = s.AccountId,
                            StudentId = s.StudentNumber,
                            BatchId = b.Id,
                            BatchNumber = b.Number,
                            SectionId = sec.Id,
                            Section = sec.Name,
                            ProgramId = s.ProgramId,
                            DepartmentId = d.Id,
                            CurrentSemester = s.CurrentSemester,
                            Email = a.Email,
                            ProgramName = p.Name,
                            DepartmentCode = d.Code
                        };

            // Step 3: Return ordered list
            return await query
                .OrderBy(s => s.StudentId)
                .AsNoTracking()
                .ToListAsync();
        }


        // Get all active students (simpler DTO)
        public async Task<List<StudentDto>> GetAllActiveStudentAsync()
        {
            var query = from s in _context.Students
                        join a in _context.Accounts on s.AccountId equals a.Id
                        join p in _context.Programs on s.ProgramId equals p.Id
                        join d in _context.Departments on p.DepartmentId equals d.Id
                        join b in _context.Batches on s.BatchId equals b.Id
                        join sec in _context.Sections on s.SectionId equals sec.Id
                        where !s.IsDeleted && a.IsActive
                        select new StudentDto
                        {
                            Id = s.Id,
                            StudentNumber = s.StudentNumber,
                            FullName = a.FirstName + " " + a.LastName,
                            DepartmentName = d.Name,
                            ProgramName = p.Name,
                            CurrentSemester = s.CurrentSemester
                        };

            return await query.ToListAsync();
        }

        public async Task<StudentDto> GetStudentAsync(Guid accountId)
        {
            var query = from s in _context.Students
                        join a in _context.Accounts on s.AccountId equals a.Id
                        join p in _context.Programs on s.ProgramId equals p.Id
                        join d in _context.Departments on p.DepartmentId equals d.Id
                        join b in _context.Batches on s.BatchId equals b.Id
                        join sec in _context.Sections on s.SectionId equals sec.Id
                        where !s.IsDeleted && a.IsActive && a.Id == accountId
                        select new StudentDto
                        {
                            Id = s.Id,
                            StudentNumber = s.StudentNumber,
                            FullName = a.FirstName + " " + a.LastName,
                            DepartmentName = d.Code,
                            ProgramName = p.Name,
                            CurrentSemester = s.CurrentSemester,
                            Batch = b.Number,
                            Section = sec.Name
                        };

            return await query.FirstOrDefaultAsync();
        }

        // Get single student
        public async Task<Student> GetStudentAsync(
            Guid? accountId = null,
            Guid? studentId = null,
            string studentCode = null)
        {
            if (accountId == null && studentId == null && string.IsNullOrEmpty(studentCode))
                throw new ArgumentException("At least one identifier must be provided.");

            var query = _context.Students
                .AsNoTracking()
                .Include(s => s.Account)
                .AsQueryable();

            if (accountId.HasValue)
                query = query.Where(s => s.AccountId == accountId.Value);

            if (studentId.HasValue)
                query = query.Where(s => s.Id == studentId.Value);

            if (!string.IsNullOrEmpty(studentCode))
                query = query.Where(s => s.StudentNumber == studentCode);

            query = query.Where(s => !s.IsDeleted && s.Account.IsActive);

            return await query.FirstOrDefaultAsync();
        }

        // Get StudentDto by ID
        public async Task<StudentDto> GetStudentByIdAsync(Guid studentId)
        {
            var query = from s in _context.Students
                        join a in _context.Accounts on s.AccountId equals a.Id
                        join p in _context.Programs on s.ProgramId equals p.Id
                        join d in _context.Departments on p.DepartmentId equals d.Id
                        join b in _context.Batches on s.BatchId equals b.Id
                        join sec in _context.Sections on s.SectionId equals sec.Id
                        // Join to get all courses in student's current semester
                        join c in _context.Curriculums
                            on new { s.ProgramId, SemesterNumber = s.CurrentSemester }
                            equals new { c.ProgramId, c.SemesterNumber } into curriculumJoin
                        from c in curriculumJoin.DefaultIfEmpty()
                            // We no longer have SemesterId in Curriculum, so remove sem join
                        where !s.IsDeleted && a.IsActive && s.Id == studentId
                        select new StudentDto
                        {
                            Id = s.Id,
                            StudentNumber = s.StudentNumber,
                            FullName = a.FirstName + " " + a.LastName,
                            ProgramName = p.Name,
                            DepartmentName = d.Code,
                            Batch = b.Number,
                            Section = sec.Name,
                            CurrentSemester = s.CurrentSemester,
                            CurrentSemesterName = $"Semester {s.CurrentSemester}"
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

            var existingIds = await _unitOfWork.Context.Students
                .Where(s => s.StudentNumber.StartsWith($"Y{admissionYear % 100:D2}"))
                .Select(s => s.StudentNumber)
                .ToListAsync();

            return _idGenerator.GenerateStudentId(admissionYear, existingIds);
        }

        public async Task<bool> DeleteAsync(Guid accountId)
        {
            await _accountService.SoftDeleteAsync(accountId);

            var student = await _unitOfWork.Context.Students
                .FirstOrDefaultAsync(s => s.AccountId == accountId);

            if (student != null)
                await LogAsync(accountId, ActionType.Delete, "Student", student.Id);

            return true;
        }
    }
}
