using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Services.Academics.Operations;
using UniPortal.Services.Accounts;
using UniPortal.Services.Dashboards;
using UniPortal.ViewModels.Dashboards;
using UniPortal.ViewModels.Operations;
using UniPortal.ViewModels.Users;

namespace UniPortal.Pages.Dashboards
{
    [Authorize(Roles = Roles.Student)]
    public class StudentModel : BasePageModel
    {
        private readonly StudentDashboardService _studentDashboardService;
        private readonly AssignmentService _assignmentService;
        private readonly StudentService _studentService;

        public StudentModel(StudentDashboardService studentDashboardService,
                              AssignmentService assignmentService,
                              AccountService accountService,
                              StudentService studentService)
            : base(accountService)
        {
            _studentDashboardService = studentDashboardService;
            _assignmentService = assignmentService;
            _studentService = studentService;
        }

        // -----------------------------
        // Properties bound to the view
        // -----------------------------
        public StudentProfileViewModel Profile { get; set; } = new();
        public List<AssignmentViewModel> Assignments { get; set; } = new();
        public MetricsViewModel Metrics { get; set; } = new();

        [BindProperty]
        public IFormFile AssignmentFile { get; set; } = null!;

        // -----------------------------
        // Page Load
        // -----------------------------
        public async Task<IActionResult> OnGetAsync()
        {
            if (CurrentAccount == null)
                return LocalRedirect("/account/login"); // redirect if not logged in

            // Get internal student Id (Student.Id) using AccountId
            var student = await _studentService.GetStudentAsync(CurrentAccount.Id);

            if (student == null)
                return NotFound("Student profile not found.");

            // Load profile and assignments
            Profile = await _studentDashboardService.GetProfileAsync(CurrentAccount.Id);
            Assignments = await _assignmentService.GetUpcomingAssignmentsAsync(student.Id, 3);

            // Load dashboard metrics (all 8 cards)
            Metrics = await _studentDashboardService.GetDashboardMetricsAsync(student.Id);

            // Make sure all metrics are populated to avoid nulls in Razor
            Metrics ??= new MetricsViewModel
            {
                Courses = 0,
                PendingAssignments = 0,
                TodayClasses = 0,
                AttendancePercent = 0,
                OverallGPA = "N/A",
                UnreadNotifications = 0,
                NotesCount = 0
            };

            return Page();
        }

        // -----------------------------
        // Assignment Submission
        // -----------------------------
        public async Task<IActionResult> OnPostSubmitAssignmentAsync(Guid assignmentId)
        {
            if (AssignmentFile == null || AssignmentFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please select a file to upload.");
                await OnGetAsync();
                return Page();
            }

            if (CurrentAccount == null)
                return Unauthorized();

            try
            {
                await _assignmentService.SubmitAssignmentAsync(assignmentId, AssignmentFile, CurrentAccount.Id);
                TempData["SuccessMessage"] = "Assignment submitted successfully!";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error uploading assignment: {ex.Message}");
            }

            await OnGetAsync();
            return Page();
        }
    }
}
