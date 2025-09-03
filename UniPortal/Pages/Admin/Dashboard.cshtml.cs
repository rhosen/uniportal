using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Services.Admin;

namespace UniPortal.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly AdminDashboardService _dashboardService;

        public DashboardModel(AdminDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int InactiveStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalCourses { get; set; }
        public int PendingNotifications { get; set; }

        public async Task OnGetAsync()
        {
            ActiveStudents = await _dashboardService.GetTotalStudentsAsync(isActive: true);
            InactiveStudents = await _dashboardService.GetTotalStudentsAsync(isActive: false);
            TotalTeachers = await _dashboardService.GetTotalTeachersAsync();
            TotalCourses = await _dashboardService.GetTotalCoursesAsync();
            PendingNotifications = await _dashboardService.GetPendingNotificationsAsync();
        }
    }

}
