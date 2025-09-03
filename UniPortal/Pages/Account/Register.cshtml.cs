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

        public RegisterModel(UserService identityService)
        {
            _userService = identityService;
        }

        [BindProperty]
        public RegistrationVM Input { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _userService.RegisterUserAsync(Input.Email, Input.Password, Roles.Student);

            if (result.Succeeded)
            {
                // Registration succeeded, redirect to login or home
                return RedirectToPage("/account/login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}
