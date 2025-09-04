using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using UniPortal.Services;

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
                CurrentAccount = await _accountService.GetByUserIdAsync(identityUserId);
            }
        }

        public string CurrentUserDisplayName => CurrentAccount != null
            ? $"{CurrentAccount.FirstName} {CurrentAccount.LastName}"
            : "Unknown";
    }
}
