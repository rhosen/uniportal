using Microsoft.AspNetCore.Authorization;
using UniPortal.Constants;
using UniPortal.Services.Accounts;
using UniPortal.Services.Dashboards;
using UniPortal.ViewModels.Users;
namespace UniPortal.Pages.Dashboards
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class AdminModel : BasePageModel
    {
        private readonly AdminDashboardService _dashboardService;
        private readonly AccountService _accountService;

        public AdminModel(AdminDashboardService dashboardService, AccountService accountService) : base(accountService)
        {
            _dashboardService = dashboardService;
            _accountService = accountService;
        }

        public AdminProfileViewModel Profile { get; set; } = new();

       
        public int TotalStudents { get; set; }
        public int OnboardedStudents { get; set; }
        public int NewStudents { get; set; }
        public int PendingStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalCourses { get; set; }

       
        public async Task OnGetAsync()
        {
            var account = await _accountService.GetAccountAsync(CurrentAccount.Id);
            if (account == null)
            {
                Response.Redirect("/account/login");
                return;
            }

            // Load admin profile for sidebar
            Profile = await _dashboardService.GetAdminProfileAsync(account.Id);

            // Load dashboard metrics
            OnboardedStudents = await _dashboardService.GetOnboardedStudentsAsync();
            NewStudents = await _dashboardService.GetNewStudentsAsync();
            PendingStudents = await _dashboardService.GetPendingStudentsAsync();
            TotalTeachers = await _dashboardService.GetTotalTeachersAsync();
            TotalCourses = await _dashboardService.GetTotalCoursesAsync();
        }
    }
}
