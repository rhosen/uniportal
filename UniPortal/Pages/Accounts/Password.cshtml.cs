using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using UniPortal.Services.Accounts;
using static UniPortal.Constants.AppConstant;

namespace UniPortal.Pages.Accounts
{
    [Authorize]
    public class PasswordModel : BasePageModel
    {
        private readonly AccountService _accountService;

        public PasswordModel(AccountService accountService) : base(accountService)
        {
            _accountService = accountService;
        }

        [BindProperty, Required]
        public string CurrentPassword { get; set; }

        [BindProperty, Required, MinLength(6)]
        public string NewPassword { get; set; }

        [BindProperty, Required, Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        public void OnGet()
        {
            
        }

        public async Task<IActionResult> OnPostChangePassword()
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _accountService.ChangePasswordAsync(CurrentAccount.Id, CurrentPassword, NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return Page();
            }

            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            // One-time message to show on login page
            TempData["Message"] = "Your password has been changed. Please log in again.";

            return RedirectToPage(AppRoutes.Login);
        }


    }
}
