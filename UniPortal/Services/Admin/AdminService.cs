using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities;

namespace UniPortal.Services.Admin
{
    public class AdminService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UniPortalContext _context;

        public AdminService(UserManager<IdentityUser> userManager,
                            RoleManager<IdentityRole> roleManager,
                            UniPortalContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // -------------------------
        // Get all active admins
        // -------------------------
        public async Task<List<Account>> GetAllAsync()
        {
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == Roles.Admin);
            if (adminRole == null) return new List<Account>();

            var adminUserIds = await _context.UserRoles
                .Where(ur => ur.RoleId == adminRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var admins = await _context.Accounts
                .Where(a => !a.IsDeleted && a.IsActive && adminUserIds.Contains(a.IdentityUserId))
                .ToListAsync();

            return admins;
        }

        // -------------------------
        // Get admin by AccountId
        // -------------------------
        public async Task<Account?> GetByIdAsync(string accountId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id.ToString() == accountId && !a.IsDeleted);

            if (account == null) return null;

            var user = await _userManager.FindByIdAsync(account.IdentityUserId);
            if (user != null && await _userManager.IsInRoleAsync(user, Roles.Admin))
                return account;

            return null;
        }

        // -------------------------
        // Create new admin
        // -------------------------
        public async Task<IdentityResult> CreateAsync(string email, string password, string firstName, string lastName)
        {
            var user = new IdentityUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return result;

            // Ensure Admin role exists
            if (!await _roleManager.RoleExistsAsync(Roles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(Roles.Admin));

            await _userManager.AddToRoleAsync(user, Roles.Admin);

            // Add to Accounts table
            var account = new Account
            {
                IdentityUserId = user.Id,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return result;
        }

        // -------------------------
        // Update admin info
        // -------------------------
        public async Task<bool> UpdateAsync(Guid accountId, string firstName, string lastName, string email)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted);
            if (account == null) return false;

            var user = await _userManager.FindByIdAsync(account.IdentityUserId);
            if (user == null) return false;

            // Update IdentityUser
            user.Email = email;
            user.UserName = email;
            var identityResult = await _userManager.UpdateAsync(user);
            if (!identityResult.Succeeded) return false;

            // Update Account info
            account.FirstName = firstName;
            account.LastName = lastName;
            account.Email = email;
            account.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // -------------------------
        // Soft delete admin
        // -------------------------
        public async Task<bool> DeleteAsync(string identityUserId)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.IdentityUserId == identityUserId && !a.IsDeleted);
            if (account == null) return false;

            account.IsActive = false;
            account.IsDeleted = true;
            account.DeletedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // -------------------------
        // Activate admin
        // -------------------------
        public async Task<bool> ActivateAsync(string identityUserId)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.IdentityUserId == identityUserId && !a.IsDeleted);
            if (account == null) return false;

            account.IsActive = true;
            account.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
