using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.ViewModel;

namespace UniPortal.Services.Student
{
    public class AssignmentService
    {
        private readonly UniPortalContext _context;

        private readonly StudentDashboardService _studentDashboardService;

        private readonly IWebHostEnvironment _env;

        public AssignmentService(UniPortalContext context,
            StudentDashboardService studentDashboardService,
            IWebHostEnvironment env)
        {
            _context = context;
            _studentDashboardService = studentDashboardService;
            _env = env;
        }

        // Get upcoming assignments
        public async Task<List<AssignmentViewModel>> GetUpcomingAssignmentsAsync(Guid studentId, int limit = 5)
        {
            var today = DateTime.Today;

            var assignments = await _context.Assignments
                .Where(a => !a.IsDeleted && a.DueDate >= today)
                .Include(a => a.Course)
                .ThenInclude(c => c.Subject)
                .OrderBy(a => a.DueDate)
                .Take(limit)
                .Select(a => new AssignmentViewModel
                {
                    Id = a.Id, // internal for submission
                    Title = a.Title,
                    CourseName = a.Course.Subject.Name,
                    DueDate = a.DueDate,
                    Status = _context.AssignmentSubmissions
                        .Any(s => s.AssignmentId == a.Id && s.StudentId == studentId)
                        ? "Submitted"
                        : (a.DueDate < DateTime.Now ? "Overdue" : "Pending")
                })
                .ToListAsync();

            return assignments;
        }

        // Main entry point
        public async Task SubmitAssignmentAsync(Guid assignmentId, IFormFile file, Guid accountId)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File is required.");

            var student = await _studentDashboardService.GetStudentAsync(accountId);
            var course = await GetCourseForAssignmentAsync(assignmentId);
            var uploadFolder = BuildUploadFolderPath(course.Subject.Code, assignmentId, student.StudentId);

            EnsureDirectoryExists(uploadFolder);

            var filePath = await SaveFileAsync(file, uploadFolder);
            var submission = await SaveSubmissionRecordAsync(assignmentId, student.Id, accountId);
            await SaveAttachmentRecordAsync(file, filePath, submission.Id, accountId);
        }

        // -----------------------------
        // Helper Methods
        // -----------------------------

      
        private async Task<Data.Entities.Course> GetCourseForAssignmentAsync(Guid assignmentId)
        {
            var course = await _context.Courses
                .Include(c => c.Subject)
                .FirstOrDefaultAsync(c => c.Id == _context.Assignments
                    .Where(a => a.Id == assignmentId)
                    .Select(a => a.CourseId)
                    .FirstOrDefault());

            if (course == null)
                throw new Exception("Course not found for this assignment.");

            return course;
        }

        private string BuildUploadFolderPath(string courseCode, Guid assignmentId, string studentId)
        {
            var root = Path.Combine(_env.WebRootPath, "uploads", "assignments");
            return Path.Combine(root, courseCode, assignmentId.ToString(), studentId);
        }

        private void EnsureDirectoryExists(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }

        private async Task<string> SaveFileAsync(IFormFile file, string folderPath)
        {
            var filePath = Path.Combine(folderPath, Path.GetFileName(file.FileName));
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            return filePath;
        }

        private async Task<Data.Entities.AssignmentSubmission> SaveSubmissionRecordAsync(Guid assignmentId, Guid studentId, Guid accountId)
        {
            var submission = new Data.Entities.AssignmentSubmission
            {
                AssignmentId = assignmentId,
                StudentId = studentId,
                SubmittedDate = DateTime.Now,
                Status = "Submitted",
                CreatedById = accountId
            };

            _context.AssignmentSubmissions.Add(submission);
            await _context.SaveChangesAsync();
            return submission;
        }

        private async Task SaveAttachmentRecordAsync(IFormFile file, string filePath, Guid submissionId, Guid accountId)
        {
            var attachment = new Data.Entities.Attachment
            {
                FileName = file.FileName,
                FilePath = filePath,
                FileType = file.ContentType,
                UploadedById = accountId,
                RelatedEntity = "AssignmentSubmission",
                RelatedEntityId = submissionId,
                CreatedAt = DateTime.Now,
                CreatedById = accountId
            };

            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();
        }
    }
}
