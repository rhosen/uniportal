using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.ViewModel;

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

        public async Task<Account?> GetAccountByIdentityIdAsync(string identityUserId)
        {
            if (string.IsNullOrWhiteSpace(identityUserId))
                return null;

            return await _dbContext.Accounts
                .AsNoTracking()   // read-only, slightly faster
                .FirstOrDefaultAsync(a => a.IdentityUserId == identityUserId);
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

        public async Task<bool> UpdateProfileAsync(ProfileViewModel profile)
        {
            if (profile == null || profile.AccountId == Guid.Empty)
                return false;

            var account = await GetByIdAsync(profile.AccountId);
            if (account == null) return false;

            account.FirstName = profile.FirstName;
            account.LastName = profile.LastName;
            account.Phone = profile.Phone;
            account.Address = profile.Address;
            account.DateOfBirth = profile.DateOfBirth;

            _dbContext.Accounts.Update(account);
            await _dbContext.SaveChangesAsync();

            await UpdateEmailAsync(account.Id, profile.Email);
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

        internal async Task<Account> GetByIdAsync(Guid accountId)
        {
            return await _dbContext.Accounts
               .FirstOrDefaultAsync(a => a.Id == accountId && a.IsActive && !a.IsDeleted);
        }

        private async Task<(IdentityUser? user, IdentityError? error)> GetIdentityUserAsync(Guid accountId)
        {
            var account = await _dbContext.Accounts.FindAsync(accountId);
            if (account == null || string.IsNullOrWhiteSpace(account.IdentityUserId))
                return (null, new IdentityError { Description = "Associated IdentityUser not found." });

            var user = await _userManager.FindByIdAsync(account.IdentityUserId);
            if (user == null)
                return (null, new IdentityError { Description = "User not found in Identity system." });

            return (user, null);
        }

        public async Task<IdentityResult> UpdatePasswordAsync(Guid accountId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                return IdentityResult.Failed(new IdentityError { Description = "Password cannot be empty." });

            var (user, error) = await GetIdentityUserAsync(accountId);
            if (user == null)
                return IdentityResult.Failed(error!);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task<IdentityResult> ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
                return IdentityResult.Failed(new IdentityError { Description = "Passwords cannot be empty." });

            var (user, error) = await GetIdentityUserAsync(accountId);
            if (user == null)
                return IdentityResult.Failed(error!);

            if (currentPassword == newPassword)
                return IdentityResult.Failed(new IdentityError { Description = "New password cannot be the same as the old password." });

            var passwordValid = await _userManager.CheckPasswordAsync(user, currentPassword);
            if (!passwordValid)
                return IdentityResult.Failed(new IdentityError { Description = "Current password is incorrect." });

            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }
    }
}
