using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Constants;
using UniPortal.Services.Accounts;
using UniPortal.ViewModels.Users;

namespace UniPortal.Pages.Accounts
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class ProfileModel : BasePageModel
    {
        private readonly AccountService _accountService;

        public ProfileModel(AccountService accountService):base(accountService)
        {
            _accountService = accountService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public Data.Entities.Account Profile { get; set; } = new();

        // For password reset
        [BindProperty]
        public string NewPassword { get; set; } = string.Empty;
        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var account = await _accountService.GetAccountAsync(id);
            if (account == null) return NotFound();

            Profile = account;
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            if (!ModelState.IsValid) return Page();
            var model = new AccountViewModel
            {
                FirstName = Profile.FirstName,
                LastName = Profile.LastName,
                Phone = Profile.Phone,
                Email = Profile.Email,
                DateOfBirth = Profile.DateOfBirth,
                Address = Profile.Address,
                AccountId = Profile.Id
            };
            await _accountService.UpdateProfileAsync(model);
            TempData["Message"] = "Profile updated successfully!";
            return Page();
        }

        public async Task<IActionResult> OnPostResetPasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                ModelState.AddModelError(nameof(NewPassword), "New password is required.");
                return Page();
            }

            if (NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError(nameof(ConfirmPassword), "Passwords do not match.");
                return Page();
            }

            var result = await _accountService.UpdatePasswordAsync(Profile.Id, NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return Page(); 
            }

            TempData["Message"] = "Password reset successfully!";
            return Page(); 
        }

    }
}
