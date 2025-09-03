using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Constants;
using UniPortal.Services.Faculty;

namespace UniPortal.Pages.Faculty
{
    [Authorize(Roles = Roles.Faculty)]
    public class DashboardModel : PageModel
    {
        private readonly FacultyDashboardService _dashboardService;

        public DashboardModel(FacultyDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; }
        public int TotalAssignments { get; set; }
        public int TotalSubmissions { get; set; }
        public int TotalNotes { get; set; }
        public int PendingAttendance { get; set; }
        public int RecentNotifications { get; set; }
        public int TotalEnrollments { get; set; }
        public int TotalScheduledClasses { get; set; }

        public async Task OnGetAsync()
        {
            var accountId = Guid.Parse(User.FindFirst("AccountId")?.Value ?? Guid.Empty.ToString());

            TotalCourses = await _dashboardService.GetTotalCoursesAsync(accountId);
            TotalStudents = await _dashboardService.GetTotalStudentsAsync(accountId);
            TotalAssignments = await _dashboardService.GetTotalAssignmentsAsync(accountId);
            TotalSubmissions = await _dashboardService.GetTotalSubmissionsAsync(accountId);
            TotalNotes = await _dashboardService.GetTotalNotesAsync(accountId);
            PendingAttendance = await _dashboardService.GetPendingAttendanceAsync(accountId);
            RecentNotifications = await _dashboardService.GetRecentNotificationsAsync(accountId);
            TotalEnrollments = await _dashboardService.GetTotalEnrollmentsAsync(accountId);
            TotalScheduledClasses = await _dashboardService.GetTotalScheduledClassesAsync(accountId);
        }
    }
}
