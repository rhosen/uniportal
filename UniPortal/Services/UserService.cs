using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;

namespace UniPortal.Services
{
    public class UserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UniPortalContext _dbContext;
        private readonly PasswordHasher<IdentityUser> _passwordHasher;

        public UserService(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,

            UniPortalContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
            _passwordHasher = new PasswordHasher<IdentityUser>();
        }


        public async Task<IdentityResult> RegisterUserAsync(string email, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Role must be specified.", nameof(role));

            // 1️⃣ Create IdentityUser
            var user = new IdentityUser
            {
                UserName = email,
                Email = email
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return result;

            // 2️⃣ Ensure role exists dynamically
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            await _userManager.AddToRoleAsync(user, role);

            // 3️⃣ Create minimal Account entry
            var account = new Account
            {
                Email = email,
                IdentityUserId = user.Id,
                IsActive = false, // student not active yet
            };

            _dbContext.Accounts.Add(account);
            await _dbContext.SaveChangesAsync();

            return result;
        }

        public async Task<SignInResult> LoginUserAsync(string email, string password, bool rememberMe = false)
        {
            return await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
        }

        public async Task LogoutUserAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<bool> IsUserInRoleAsync(IdentityUser user, string role)
        {
            return await _userManager.IsInRoleAsync(user, role);
        }

        public async Task<bool> VerifyPasswordAsync(string email, string password)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }

        public async Task<IdentityUser?> GetUserByEmailAsync(string email)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IList<string>> GetUserRoleAsync(IdentityUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<IdentityResult> ChangePasswordAsync(IdentityUser user, string currentPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

    }
}
