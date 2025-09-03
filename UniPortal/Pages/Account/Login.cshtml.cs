using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
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



        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // 1️⃣ Validate user credentials via IdentityService
            var user = await _userService.GetUserByEmailAsync(Input.Email);
            if (user == null || !await _userService.VerifyPasswordAsync(Input.Email, Input.Password))
                return InvalidLogin("Invalid login attempt.");

            var roles = await _userService.GetUserRoleAsync(user);
            var role = roles.FirstOrDefault();

            // 2️⃣ Get active account via AccountService
            var account = await _accountService.GetActiveAccountAsync(user.Id);
            if (account == null)
                return InvalidLogin("Your account is not yet activated. Please contact admin.");

            // 3️⃣ Determine display name
            string displayName = !string.IsNullOrWhiteSpace(account.FirstName + account.LastName)
                ? $"{account.FirstName} {account.LastName}".Trim()
                : user.Email!.Split('@')[0];

            // 4️⃣ Create claims with dynamic role
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, displayName),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, role) // role from account
            };

            // 5️⃣ Sign in with cookie
            var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            await HttpContext.SignInAsync(
                IdentityConstants.ApplicationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = Input.RememberMe,
                    ExpiresUtc = DateTimeOffset.Now.AddHours(8)
                });

            // 6️⃣ Role-based redirection
            var redirectUrl = role switch
            {
                "Admin" => "/admin/dashboard",
                "Faculty" => "/faculty/dashboard",
                "Coordinator" => "/faculty/dashboard",
                _ => "/student/dashboard"
            };

            return LocalRedirect(redirectUrl);
        }

        // ---------------- Helper ----------------
        private IActionResult InvalidLogin(string message)
        {
            ModelState.AddModelError(string.Empty, message);
            return Page();
        }

    }
}
