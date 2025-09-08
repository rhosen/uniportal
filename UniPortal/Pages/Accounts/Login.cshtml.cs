using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Services.Accounts;
using UniPortal.ViewModels.Accounts;
using static UniPortal.Constants.AppConstant;

namespace UniPortal.Pages.Accounts
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
        public LoginViewModel Input { get; set; } = new();

        // This method is used for both GET and POST to validate the user
        private async Task<(bool IsValid, string ErrorMessage, string DisplayName, string Role, string IdentityId)> ValidateUserAsync(string email, string password)
        {
            // 1️⃣ Validate user credentials
            var user = await _userService.GetUserByEmailAsync(email);
            var credentialsValid = user != null && await _userService.VerifyPasswordAsync(email, password);

            // 2️⃣ Get account info if user exists
            Data.Entities.Account account = null;
            if (user != null)
                account = await _accountService.GetAccountAsync(null, user.Id);

            // 4️⃣ For all other invalid cases → generic message
            if (!credentialsValid || account == null || account.IsDeleted)
                return (false, "Invalid login.", string.Empty, string.Empty, string.Empty);

            // 3️⃣ Account exists but not activated → specific message
            if (account != null && !account.IsActive)
                return (false, "Your account is not yet activated. Please contact the administrator.", string.Empty, string.Empty, string.Empty);

            // 5️⃣ Determine display name
            string displayName = !string.IsNullOrWhiteSpace(account.FirstName + account.LastName)
                ? $"{account.FirstName} {account.LastName}".Trim()
                : user.Email!.Split('@')[0];

            // 6️⃣ Get roles
            var roles = await _userService.GetUserRoleAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;

            return (true, string.Empty, displayName, role, user.Id.ToString());
        }


        // Helper method to handle redirection based on role
        private IActionResult RedirectToRoleBasedPage(string role)
        {
            var redirectUrl = role switch
            {
                Roles.Root => AppRoutes.AdminDashboard,
                Roles.Admin => AppRoutes.AdminDashboard,
                Roles.Faculty => AppRoutes.FacultyDashboard,
                Roles.Student => AppRoutes.StudentDashboard,
                _ => AppRoutes.Login
            };

            return LocalRedirect(redirectUrl);
        }

        public Task<IActionResult> OnGetAsync()
        {
            // Check if the user is already authenticated
            if (User.Identity.IsAuthenticated)
            {
                // Get the user's role (if it's available)
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                // Redirect based on the role using the helper method
                return Task.FromResult(RedirectToRoleBasedPage(role) as IActionResult);
            }

            // If the user is not authenticated, just return the login page
            return Task.FromResult(Page() as IActionResult);
        }


        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // 1️⃣ Use the ValidateUserAsync method
            var (isValid, errorMessage, displayName, role, identityId) = await ValidateUserAsync(Input.Email, Input.Password);

            if (!isValid)
                return InvalidLogin(errorMessage);

            // 2️⃣ Create claims with dynamic role
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, displayName),
                new Claim(ClaimTypes.Email, Input.Email),
                new Claim(ClaimTypes.NameIdentifier, identityId),
                new Claim(ClaimTypes.Role, role),
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
