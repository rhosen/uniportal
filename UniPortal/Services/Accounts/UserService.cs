using Microsoft.AspNetCore.Identity;
using UniPortal.Data;

namespace UniPortal.Services.Accounts
{
    public class UserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly PasswordHasher<IdentityUser> _passwordHasher;

        public UserService(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _passwordHasher = new PasswordHasher<IdentityUser>();
        }

        // Register IdentityUser and assign role
        public async Task<IdentityUser> RegisterUserAsync(string email, string password, string role)
        {
            var user = new IdentityUser { UserName = email, Email = email };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));

            await _userManager.AddToRoleAsync(user, role);

            return user;
        }


        // Update IdentityUser
        public async Task<IdentityResult> UpdateAsync(IdentityUser user)
        {
            return await _userManager.UpdateAsync(user);
        }

        // Password update via Identity
        public async Task<IdentityResult> UpdatePasswordAsync(IdentityUser user, string newPassword)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task<IdentityResult> ChangePasswordAsync(IdentityUser user, string currentPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        // Verify plain password against IdentityUser's hashed password
        public async Task<bool> VerifyPasswordAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }


        // Get IdentityUser by Id
        public async Task<IdentityUser> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        // Get IdentityUser by email
        public async Task<IdentityUser> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IList<IdentityUser>> GetUsersInRoleAsync(string role)
        {
            return await _userManager.GetUsersInRoleAsync(role);
        }

        public async Task<IList<string>> GetUserRoleAsync(IdentityUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            return await _userManager.GetRolesAsync(user);
        }

     
    }
}
