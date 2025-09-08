using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Services.Accounts;
using UniPortal.Services.Dashboards;
using UniPortal.ViewModels.Dashboards;
using UniPortal.ViewModels.Users;

namespace UniPortal.Pages.Dashboards
{
    [Authorize(Roles = Roles.Faculty)]
    public class FacultyModel : BasePageModel
    {
        private readonly FacultyDashboardService _facultyDashboardService;

        public FacultyModel(FacultyDashboardService facultyDashboardService,
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
