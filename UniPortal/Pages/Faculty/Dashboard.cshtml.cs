using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Services;
using UniPortal.Services.Faculty;
using UniPortal.ViewModel;

namespace UniPortal.Pages.Faculty
{
    [Authorize(Roles = Roles.Faculty)]
    public class DashboardModel : BasePageModel
    {
        private readonly FacultyDashboardService _facultyDashboardService;

        public DashboardModel(FacultyDashboardService facultyDashboardService,
                              AccountService accountService)
            : base(accountService)
        {
            _facultyDashboardService = facultyDashboardService;
        }

        // -----------------------------
        // Properties bound to the view
        // -----------------------------
        public FacultyProfileViewModel Profile { get; set; } = new();
        public FacultyMetricsViewModel Metrics { get; set; } = new();

        // -----------------------------
        // Page Load
        // -----------------------------
        public async Task<IActionResult> OnGetAsync()
        {
            if (CurrentAccount == null)
                return LocalRedirect("/account/login");

            // Load faculty profile using account ID
            var faculty = await _facultyDashboardService.GetFacultyProfileAsync(CurrentAccount.Id);
            if (faculty == null)
                return NotFound("Faculty profile not found.");

            Profile = faculty;

            // Load dashboard metrics
            Metrics = await _facultyDashboardService.GetDashboardMetricsAsync(CurrentAccount.Id);

            // Ensure metrics are populated
            Metrics ??= new FacultyMetricsViewModel
            {
                TotalCourses = 0,
                UpcomingClass = "N/A"
            };

            return Page();
        }
    }
}
