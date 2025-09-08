using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UniPortal.Pages.Account
{
    [Authorize]
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnPostAsync()
        {
            // Sign out the user using Identity's default scheme
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            // Optional: clear cookies or temp data if needed
            HttpContext.Response.Cookies.Delete(".AspNetCore.Identity.Application");

            // Redirect to login or home page
            return RedirectToPage("/account/login");
        }
    }
}
