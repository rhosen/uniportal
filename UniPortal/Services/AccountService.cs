using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;

namespace UniPortal.Services
{
    public class AccountService
    {
        private readonly UniPortalContext _dbContext;

        public AccountService(UniPortalContext dbContext)
        {
            _dbContext = dbContext;
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
            account.DeletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        // Activate account (admin action)
        public async Task ActivateAsync(Guid accountId)
        {
            var account = await _dbContext.Accounts.FindAsync(accountId);
            if (account == null) return;

            account.IsActive = true;
            account.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        // Optional: update profile
        public async Task UpdateProfileAsync(Account account)
        {
            account.UpdatedAt = DateTime.UtcNow;
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
            account.PhoneNumber = phone;
            account.Address = address;

            _dbContext.Accounts.Update(account);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }

}
