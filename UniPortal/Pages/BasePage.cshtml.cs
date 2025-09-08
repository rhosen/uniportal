using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using UniPortal.Constants;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages
{
    public class BasePageModel : PageModel
    {
        private readonly AccountService _accountService;

        public BasePageModel(AccountService accountService)
        {
            _accountService = accountService;
        }

        public Data.Entities.Account CurrentAccount { get; private set; }

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            await LoadCurrentAccountAsync();
            await next();
        }

        protected async Task LoadCurrentAccountAsync()
        {
            var identityUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(identityUserId))
            {
                CurrentAccount = await _accountService.GetAccountAsync(null, identityUserId);
            }
        }

        public string CurrentUserDisplayName => CurrentAccount != null
            ? $"{CurrentAccount.FirstName} {CurrentAccount.LastName}"
            : "Unknown";

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
    }
}
