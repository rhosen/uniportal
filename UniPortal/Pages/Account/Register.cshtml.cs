using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using UniPortal.Auth;
using UniPortal.Services;
using UniPortal.ViewModel;

namespace UniPortal.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserService _identityService;

        public RegisterModel(UserService identityService)
        {
            _identityService = identityService;
        }

        [BindProperty]
        public RegistrationVM Input { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _identityService.RegisterUserAsync(Input.Email, Input.Password, Roles.Student);

            if (result.Succeeded)
            {
                // Registration succeeded, redirect to login or home
                return RedirectToPage("/Account/Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}
