using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Services.Accounts;
using UniPortal.ViewModels.Operations;

namespace UniPortal.Services.Academics.Operations
{
    public class AssignmentService
    {
        private readonly UniPortalContext _context;
        private readonly StudentService _studentService;
        private readonly IWebHostEnvironment _env;

        public AssignmentService(
            UniPortalContext context,
            StudentService studentService,
            IWebHostEnvironment env)
        {
            _context = context;
            _studentService = studentService;
            _env = env;
        }

        // -----------------------------
        // Get upcoming assignments for a student
        // -----------------------------
        public async Task<List<AssignmentViewModel>> GetUpcomingAssignmentsAsync(Guid studentId, int limit = 5)
        {
            var today = DateTime.Today;

            var query = from a in _context.Classworks
                        join co in _context.CourseOfferings on a.CourseOfferingId equals co.Id
                        join crs in _context.Courses on co.CourseId equals crs.Id
                        join e in _context.Enrollments on co.Id equals e.CourseOfferingId
                        where !a.IsDeleted && !co.IsDeleted && !e.IsDeleted
                              && e.StudentId == studentId
                              && a.RequiresSubmission
                              && a.DueDate >= today
                        orderby a.DueDate
                        select new AssignmentViewModel
                        {
                            Id = a.Id,
                            Title = a.Title,
                            CourseName = crs.Title,
                            DueDate = a.DueDate,
                            Status = _context.ClassworkSubmissions
                                .Any(s => s.ClassworkId == a.Id && s.StudentId == studentId && !s.IsDeleted)
                                ? "Submitted"
                                : a.DueDate < DateTime.Now ? "Overdue" : "Pending"
                        };

            return await query.Take(limit).ToListAsync();
        }

        // -----------------------------
        // Create a new assignment (optional file)
        // -----------------------------
        public async Task CreateAssignmentAsync(Guid courseOfferingId, string title, string description, IFormFile? file, Guid accountId)
        {
            string? filePath = null;

            if (file != null && file.Length > 0)
            {
                var uploadFolder = Path.Combine(_env.WebRootPath, "uploads", "assignments", courseOfferingId.ToString());
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                filePath = Path.Combine(uploadFolder, Path.GetFileName(file.FileName));
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
            }

            var assignment = new Classwork
            {
                CourseOfferingId = courseOfferingId,
                Title = title,
                Description = description,
                FilePath = filePath,
                RequiresSubmission = true, // or pass as parameter if optional
                DueDate = DateTime.Today.AddDays(7), // example default, can pass parameter
                ModifiedById = accountId
            };

            _context.Classworks.Add(assignment);
            await _context.SaveChangesAsync();
        }

        // -----------------------------
        // Submit assignment for a student (file required)
        // -----------------------------
        public async Task SubmitAssignmentAsync(Guid assignmentId, IFormFile file, Guid accountId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is required.");

            var student = await _studentService.GetStudentAsync(accountId);
            if (student == null)
                throw new Exception("Student not found.");

            var uploadFolder = Path.Combine(_env.WebRootPath, "uploads", "assignments", assignmentId.ToString(), student.StudentNumber);
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var filePath = Path.Combine(uploadFolder, Path.GetFileName(file.FileName));
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var existingSubmission = await _context.ClassworkSubmissions
                .FirstOrDefaultAsync(s => s.ClassworkId == assignmentId && s.StudentId == student.Id && !s.IsDeleted);

            if (existingSubmission != null)
            {
                existingSubmission.FilePath = filePath;
                existingSubmission.UpdatedAt = DateTime.Now;
                existingSubmission.ModifiedById = accountId;
            }
            else
            {
                var submission = new ClassworkSubmission
                {
                    ClassworkId = assignmentId,
                    StudentId = student.Id,
                    FilePath = filePath,
                    CreatedAt = DateTime.Now,
                    ModifiedById = accountId
                };
                _context.ClassworkSubmissions.Add(submission);
            }

            await _context.SaveChangesAsync();
        }

        // -----------------------------
        // Get assignment details including submission status
        // -----------------------------
        public async Task<AssignmentViewModel?> GetAssignmentDetailsAsync(Guid assignmentId, Guid studentId)
        {
            var assignment = await (from a in _context.Classworks
                                    join co in _context.CourseOfferings on a.CourseOfferingId equals co.Id
                                    join crs in _context.Courses on co.CourseId equals crs.Id
                                    where a.Id == assignmentId && !a.IsDeleted && !co.IsDeleted
                                    select new AssignmentViewModel
                                    {
                                        Id = a.Id,
                                        Title = a.Title,
                                        Description = a.Description,
                                        CourseName = crs.Title,
                                        DueDate = a.DueDate,
                                        Status = _context.ClassworkSubmissions
                                            .Any(s => s.ClassworkId == a.Id && s.StudentId == studentId && !s.IsDeleted)
                                            ? "Submitted"
                                            : a.RequiresSubmission ? "Pending" : "N/A"
                                    }).FirstOrDefaultAsync();

            return assignment;
        }

    }
}
