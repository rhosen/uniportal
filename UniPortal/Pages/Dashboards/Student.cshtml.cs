using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Dtos;
using UniPortal.Services.Accounts;
using UniPortal.Services.Dashboards;
using UniPortal.ViewModels.Users;

namespace UniPortal.Pages.Dashboards
{
    [Authorize(Roles = Roles.Student)]
    public class StudentModel : BasePageModel
    {
        private readonly StudentDashboardService _studentDashboardService;
        private readonly StudentService _studentService;

        public StudentModel(StudentDashboardService studentDashboardService,
                              AccountService accountService,
                              StudentService studentService)
            : base(accountService)
        {
            _studentDashboardService = studentDashboardService;
            _studentService = studentService;
        }

        // -----------------------------
        // Properties bound to the view
        // -----------------------------
        public StudentProfileViewModel Profile { get; set; } = new();
        public StudentMetricDto Metrics { get; set; } = new();

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

            // Load dashboard metrics (all 8 cards)
            Metrics = await _studentDashboardService.GetDashboardMetricsAsync(student.Id);

            // Make sure all metrics are populated to avoid nulls in Razor
            Metrics ??= new StudentMetricDto
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
    }
}
