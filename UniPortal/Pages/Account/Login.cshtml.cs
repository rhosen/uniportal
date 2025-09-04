using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Services;
using UniPortal.ViewModel;

namespace UniPortal.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly UserService _userService;
        private readonly AccountService _accountService;
        private readonly UniPortalContext _dbContext;

        public LoginModel(
            UserService identityService,
            AccountService accountService,
            UniPortalContext dbContext)
        {
            _userService = identityService;
            _accountService = accountService;
            _dbContext = dbContext;
        }

        [BindProperty]
        public LoginVM Input { get; set; } = new();

        // This method is used for both GET and POST to validate the user
        private async Task<(bool IsValid, string ErrorMessage, string DisplayName, string Role)> ValidateUserAsync(string email, string password)
        {
            // 1️⃣ Validate user credentials via IdentityService
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null || !await _userService.VerifyPasswordAsync(email, password))
                return (false, "Invalid login attempt.", string.Empty, string.Empty);

            // 2️⃣ Get active account via AccountService
            var account = await _accountService.GetActiveAccountAsync(user.Id);
            if (account == null)
                return (false, "Your account is not yet activated. Please contact admin.", string.Empty, string.Empty);

            // 3️⃣ Determine display name
            string displayName = !string.IsNullOrWhiteSpace(account.FirstName + account.LastName)
                ? $"{account.FirstName} {account.LastName}".Trim()
                : user.Email!.Split('@')[0];

            // 4️⃣ Get roles for the user
            var roles = await _userService.GetUserRoleAsync(user);
            var role = roles.FirstOrDefault();

            return (true, string.Empty, displayName, role);
        }

        // Helper method to handle redirection based on role
        private IActionResult RedirectToRoleBasedPage(string role)
        {
            var redirectUrl = role switch
            {
                Roles.Admin => "/admin/dashboard",
                Roles.Faculty => "/faculty/dashboard",
                Roles.Student => "/student/dashboard",
                _ => "/home/index" // Default redirection if no matching role is found
            };

            return LocalRedirect(redirectUrl);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if the user is already authenticated
            if (User.Identity.IsAuthenticated)
            {
                // Get the user's role (if it's available)
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                // Redirect based on the role using the helper method
                return RedirectToRoleBasedPage(role);
            }

            // If the user is not authenticated, just return the login page
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // 1️⃣ Use the ValidateUserAsync method
            var (isValid, errorMessage, displayName, role) = await ValidateUserAsync(Input.Email, Input.Password);

            if (!isValid)
                return InvalidLogin(errorMessage);

            // 2️⃣ Create claims with dynamic role
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, displayName),
                new Claim(ClaimTypes.Email, Input.Email),
                new Claim(ClaimTypes.NameIdentifier, displayName),
                new Claim(ClaimTypes.Role, role) // role from account
            };

            // 3️⃣ Sign in with cookie
            var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            await HttpContext.SignInAsync(
                IdentityConstants.ApplicationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = Input.RememberMe,
                    ExpiresUtc = DateTimeOffset.Now.AddHours(8)
                });

            // 4️⃣ Redirect based on the role using the helper method
            return RedirectToRoleBasedPage(role);
        }

        // ---------------- Helper ----------------
        private IActionResult InvalidLogin(string message)
        {
            ModelState.AddModelError(string.Empty, message);
            return Page();
        }
    }
}
