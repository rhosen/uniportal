using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities;

namespace UniPortal.Services.Faculty
{
    public class TeacherService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UniPortalContext _context;

        public TeacherService(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            UniPortalContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // Get all active teachers
        public async Task<List<Account>> GetAllAsync()
        {
            // Get the role ID for Faculty
            var facultyRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == Roles.Faculty);
            if (facultyRole == null) return new List<Account>();

            // Get all user IDs in Faculty role
            var facultyUserIds = await _context.UserRoles
                .Where(ur => ur.RoleId == facultyRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            // Get Accounts linked to those users
            var teachers = await _context.Accounts
                .Where(a => !a.IsDeleted && a.IsActive && facultyUserIds.Contains(a.IdentityUserId))
                .ToListAsync();

            return teachers;
        }

        // Get teacher by AccountId or IdentityUserId
        public async Task<Account> GetByIdAsync(string accountId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id.ToString() == accountId && !a.IsDeleted && a.IsActive);

            if (account == null) return null;

            var user = await _userManager.FindByIdAsync(account.IdentityUserId);
            if (user != null && await _userManager.IsInRoleAsync(user, Roles.Faculty))
                return account;

            return null;
        }

        // Create new teacher (IdentityUser + Accounts)
        public async Task<IdentityResult> CreateAsync(string email, string password, string firstName, string lastName)
        {
            var user = new IdentityUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return result;

            // Ensure Faculty role exists
            if (!await _roleManager.RoleExistsAsync(Roles.Faculty))
                await _roleManager.CreateAsync(new IdentityRole(Roles.Faculty));

            await _userManager.AddToRoleAsync(user, Roles.Faculty);

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

        // Update teacher info
        public async Task<bool> UpdateAsync(Guid accoundId, string firstName, string lastName, string email)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == accoundId && !a.IsDeleted);

            if (account == null) return false;

            var user = await _userManager.FindByIdAsync(account.IdentityUserId);
            if (user == null) return false;

            // Update IdentityUser email
            user.Email = email;
            user.UserName = email;
            var identityResult = await _userManager.UpdateAsync(user);
            if (!identityResult.Succeeded) return false;

            // Update Accounts info
            account.FirstName = firstName;
            account.LastName = lastName;
            account.Email = email;
            account.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // Soft delete teacher
        public async Task<bool> DeleteAsync(string identityUserId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.IdentityUserId == identityUserId && !a.IsDeleted);

            if (account == null) return false;

            account.IsActive = false;
            account.IsDeleted = true;
            account.DeletedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // Activate teacher
        public async Task<bool> ActivateAsync(string identityUserId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.IdentityUserId == identityUserId && !a.IsDeleted);

            if (account == null) return false;

            account.IsActive = true;
            account.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
