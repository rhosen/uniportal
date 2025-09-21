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

            var query = from a in _context.Assignments
                        join co in _context.CourseOfferings on a.CourseOfferingId equals co.Id
                        join crs in _context.Courses on co.CourseId equals crs.Id
                        join e in _context.Enrollments on co.Id equals e.CourseOfferingId
                        where !a.IsDeleted && !co.IsDeleted && !e.IsDeleted
                              && e.StudentId == studentId
                              && a.DueDate >= today
                        orderby a.DueDate
                        select new AssignmentViewModel
                        {
                            Id = a.Id,
                            Title = a.Title,
                            CourseName = crs.Title, // join used here
                            DueDate = a.DueDate,
                            Status = _context.AssignmentSubmissions
                                .Any(s => s.AssignmentId == a.Id && s.StudentId == studentId && !s.IsDeleted)
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

            var assignment = new Assignment
            {
                CourseOfferingId = courseOfferingId,
                Title = title,
                Description = description,
                FilePath = filePath,
                ModifiedById = accountId
            };

            _context.Assignments.Add(assignment);
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

            // Build folder path: /wwwroot/uploads/assignments/{assignmentId}/{studentNumber}/
            var uploadFolder = Path.Combine(_env.WebRootPath, "uploads", "assignments", assignmentId.ToString(), student.StudentNumber);
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var filePath = Path.Combine(uploadFolder, Path.GetFileName(file.FileName));
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Check if submission already exists
            var existingSubmission = await _context.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == student.Id && !s.IsDeleted);

            if (existingSubmission != null)
            {
                // Update existing submission
                existingSubmission.FilePath = filePath;
                existingSubmission.SubmittedDate = DateTime.Now;
                existingSubmission.Status = "Submitted";
                existingSubmission.ModifiedById = accountId;
            }
            else
            {
                // New submission
                var submission = new AssignmentSubmission
                {
                    AssignmentId = assignmentId,
                    StudentId = student.Id,
                    FilePath = filePath,
                    SubmittedDate = DateTime.Now,
                    Status = "Submitted",
                    ModifiedById = accountId
                };
                _context.AssignmentSubmissions.Add(submission);
            }

            await _context.SaveChangesAsync();
        }

        // -----------------------------
        // Get assignment details including submission status
        // -----------------------------
        public async Task<AssignmentViewModel?> GetAssignmentDetailsAsync(Guid assignmentId, Guid studentId)
        {
            var assignment = await (from a in _context.Assignments
                                    join co in _context.CourseOfferings on a.CourseOfferingId equals co.Id
                                    join crs in _context.Courses on co.CourseId equals crs.Id   // fetch the actual course title
                                    where a.Id == assignmentId && !a.IsDeleted && !co.IsDeleted
                                    select new AssignmentViewModel
                                    {
                                        Id = a.Id,
                                        Title = a.Title,
                                        Description = a.Description,
                                        CourseName = crs.Title, // <- correctly get course title
                                        DueDate = a.DueDate,
                                        Status = _context.AssignmentSubmissions
                                            .Any(s => s.AssignmentId == a.Id && s.StudentId == studentId && !s.IsDeleted)
                                            ? "Submitted"
                                            : "Pending"
                                    }).FirstOrDefaultAsync();

            return assignment;
        }

    }
}
