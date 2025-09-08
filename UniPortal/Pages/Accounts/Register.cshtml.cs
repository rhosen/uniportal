using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Constants;
using UniPortal.Services.Accounts;
using UniPortal.ViewModels.Accounts;

namespace UniPortal.Pages.Accounts
{
    public class RegisterModel : PageModel
    {
        private readonly AccountService _accountService;

        public RegisterModel(AccountService accountService)
        {
            _accountService = accountService;
        }

        [BindProperty]
        public RegistrationViewModel Input { get; set; }

        public bool RegistrationSucceeded { get; set; }  // <-- Flag to show success message

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                // Use AccountService to create both IdentityUser and Account
                var account = await _accountService.CreateAccountAsync(
                    Input.Email,
                    Input.Password,
                    Roles.Student
                );

                RegistrationSucceeded = true; // <-- success flag
                return Page();
            }
            catch (Exception ex)
            {
                // Show friendly error messages
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }
    }
}
