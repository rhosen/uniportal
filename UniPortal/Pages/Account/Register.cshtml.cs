using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Constants;
using UniPortal.Services;
using UniPortal.ViewModel;

namespace UniPortal.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserService _userService;

        public RegisterModel(UserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public RegistrationVM Input { get; set; }

        public bool RegistrationSucceeded { get; set; }  // <-- Make sure this is here

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _userService.RegisterUserAsync(Input.Email, Input.Password, Roles.Student);

            if (result.Succeeded)
            {
                RegistrationSucceeded = true; // <-- Set this to show message
                return Page();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}
