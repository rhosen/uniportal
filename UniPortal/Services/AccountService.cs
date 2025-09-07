using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities;

namespace UniPortal.Services
{
    public class AccountService
    {
        private readonly UniPortalContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        public AccountService(UniPortalContext dbContext, UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        // Create new account (linked to existing IdentityUser)
        public async Task<Account> CreateAccountAsync(string identityUserId, string email, string? firstName = null, string? lastName = null)
        {
            var account = new Account
            {
                IdentityUserId = identityUserId,
                Email = email,
                FirstName = firstName ?? "",
                LastName = lastName ?? "",
                IsActive = false, // student must be activated by admin
                IsDeleted = false
            };

            _dbContext.Accounts.Add(account);
            await _dbContext.SaveChangesAsync();
            return account;
        }

        // Get active account by IdentityUserId
        public async Task<Account?> GetActiveAccountAsync(string identityUserId)
        {
            return await _dbContext.Accounts
                .FirstOrDefaultAsync(a => a.IdentityUserId == identityUserId && a.IsActive && !a.IsDeleted);
        }

        // Soft delete account
        public async Task SoftDeleteAsync(Guid accountId)
        {
            var account = await _dbContext.Accounts.FindAsync(accountId);
            if (account == null) return;

            account.IsDeleted = true;
            account.DeletedAt = DateTime.Now;
            await _dbContext.SaveChangesAsync();
        }

        // Activate account (admin action)
        public async Task ActivateAsync(Guid accountId)
        {
            var account = await _dbContext.Accounts.FindAsync(accountId);
            if (account == null) return;

            account.IsActive = true;
            account.UpdatedAt = DateTime.Now;
            await _dbContext.SaveChangesAsync();
        }

        // Optional: update profile
        public async Task UpdateProfileAsync(Account account)
        {
            account.UpdatedAt = DateTime.Now;
            _dbContext.Accounts.Update(account);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Account?> GetByUserIdAsync(string userId)
        {
            return await _dbContext.Accounts.FirstOrDefaultAsync(a => a.IdentityUserId == userId && !a.IsDeleted);
        }

        public async Task<bool> UpdateProfileAsync(string userId, string firstName, string lastName, string phone, string address)
        {
            var account = await GetByUserIdAsync(userId);
            if (account == null) return false;

            account.FirstName = firstName;
            account.LastName = lastName;
            account.Phone = phone;
            account.Address = address;

            _dbContext.Accounts.Update(account);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<Account>> GetInactiveStudentsAsync()
        {
            // Get the user IDs of students (in memory)
            var studentIds = (await _userManager.GetUsersInRoleAsync(Roles.Student))
                             .Select(u => u.Id)
                             .ToList();

            // Query the Accounts filtering with the studentIds list
            return await _dbContext.Accounts
                .Where(a => studentIds.Contains(a.IdentityUserId) && !a.IsDeleted && !a.IsActive)
                .ToListAsync();
        }

        public async Task<List<Account>> GetActiveStudentsAsync()
        {
            // Get the user IDs of students (in memory)
            var studentIds = (await _userManager.GetUsersInRoleAsync(Roles.Student))
                             .Select(u => u.Id)
                             .ToList();

            // Query the Accounts filtering with the studentIds list
            return await _dbContext.Accounts
                .Where(a => studentIds.Contains(a.IdentityUserId) && !a.IsDeleted && a.IsActive)
                .ToListAsync();
        }

        public async Task UpdateEmailAsync(Guid accountId, string newEmail)
        {
            var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
            if (account == null) return;

            // Update AspNetUsers
            var identityUser = await _userManager.FindByIdAsync(account.IdentityUserId);
            if (identityUser != null && identityUser.Email != newEmail)
            {
                identityUser.Email = newEmail;
                identityUser.UserName = newEmail; // keep username in sync if needed
                await _userManager.UpdateAsync(identityUser);
            }

            // Update Accounts table
            account.Email = newEmail;
            account.UpdatedAt = DateTime.UtcNow;
            _dbContext.Accounts.Update(account);
            await _dbContext.SaveChangesAsync();
        }
    }
}
