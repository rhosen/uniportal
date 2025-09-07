using Microsoft.AspNetCore.Authorization;
using UniPortal.Constants;
using UniPortal.Services;
using UniPortal.Services.Admin;
using UniPortal.ViewModel; 
namespace UniPortal.Pages.Admin
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class DashboardModel : BasePageModel
    {
        private readonly AdminDashboardService _dashboardService;
        private readonly AccountService _accountService;

        public DashboardModel(AdminDashboardService dashboardService, AccountService accountService) : base(accountService)
        {
            _dashboardService = dashboardService;
            _accountService = accountService;
        }

        public AdminProfileViewModel Profile { get; set; } = new();

       
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int InactiveStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalCourses { get; set; }

       
        public async Task OnGetAsync()
        {
            var account = await _accountService.GetByIdAsync(CurrentAccount.Id);
            if (account == null)
            {
                Response.Redirect("/account/login");
                return;
            }

            // Load admin profile for sidebar
            Profile = await _dashboardService.GetAdminProfileAsync(account.Id);

            // Load dashboard metrics
            ActiveStudents = await _dashboardService.GetTotalStudentsAsync(isActive: true);
            InactiveStudents = await _dashboardService.GetTotalStudentsAsync(isActive: false);
            TotalTeachers = await _dashboardService.GetTotalTeachersAsync();
            TotalCourses = await _dashboardService.GetTotalCoursesAsync();
        }
    }
}
