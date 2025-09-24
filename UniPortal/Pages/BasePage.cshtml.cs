using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using UniPortal.Constants;
using UniPortal.Services.Accounts;
using static UniPortal.Constants.AppConstant;

namespace UniPortal.Pages
{
    public class BasePageModel : PageModel
    {
        private readonly AccountService _accountService;

        [TempData]
        public string? StatusMessage { get; set; }

        [TempData]
        public string? StatusMessageType { get; set; } // "success", "warning", "danger", "info"

        public BasePageModel(AccountService accountService)
        {
            _accountService = accountService;
        }

        // Always initialized to prevent NullReferenceException
        public Data.Entities.Account CurrentAccount { get; private set; } = new();

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            if (!User.Identity.IsAuthenticated)
            {
                // Redirect to login page if user is not logged in
                context.Result = RedirectToPage(AppRoutes.Login);
                return;
            }

            await LoadCurrentAccountAsync();
            await next();
        }

        protected async Task LoadCurrentAccountAsync()
        {
            var identityUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(identityUserId))
            {
                var account = await _accountService.GetAccountAsync(null, identityUserId);
                if (account != null)
                {
                    CurrentAccount = account;
                }
            }
        }

        public string CurrentUserDisplayName => $"{CurrentAccount.FirstName} {CurrentAccount.LastName}".Trim();

        public string LayoutForRole
        {
            get
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                return role switch
                {
                    Roles.Root => "_AdminLayout",
                    Roles.Admin => "_AdminLayout",
                    Roles.Faculty => "_FacultyLayout",
                    Roles.Student => "_StudentLayout",
                    _ => "_Layout"
                };
            }
        }

        // Helper method for running async actions with message handling
        protected Task R(Func<Task> action, string msg) => RunWithMessageAsync(action, msg);

        public async Task RunWithMessageAsync(Func<Task> action, string successMessage = "Action completed successfully.")
        {
            try
            {
                await action.Invoke();
                StatusMessage = successMessage;
                StatusMessageType = "success";
            }
            catch (InvalidOperationException ex)
            {
                StatusMessage = ex.Message;
                StatusMessageType = "warning";
            }
        }
    }
}
