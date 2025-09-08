using UniPortal.Data.Entities;
using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.ViewModels.Users;

namespace UniPortal.Services.Accounts
{
    public class AdminService
    {
        private readonly AccountService _accountService;
        private readonly UniPortal.Data.UniPortalContext _context;

        public AdminService(AccountService accountService, UniPortal.Data.UniPortalContext context)
        {
            _accountService = accountService;
            _context = context;
        }


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


        public async Task<Account> GetByIdAsync(Guid accountId)
        {
            return await _accountService.GetAccountAsync(accountId);
        }


        public async Task CreateAsync(string email, string password, string firstName, string lastName)
        {
            await _accountService.CreateAccountAsync(email, password, Roles.Admin, firstName, lastName);
        }


        public async Task<bool> UpdateAsync(AccountViewModel profile)
        {
            var account = await _accountService.GetAccountAsync(accountId: profile.AccountId);
            if (account == null) return false;

            // Update email if changed
            if (account.Email != profile.Email)
                await _accountService.UpdateEmailAsync(profile.AccountId, profile.Email);

            // Update full profile
            return await _accountService.UpdateProfileAsync(profile);
        }


        public async Task DeleteAsync(Guid accountId)
        {
            await _accountService.SoftDeleteAsync(accountId);
        }

        public async Task ActivateAsync(Guid accountId)
        {
            var account = await _accountService.GetAccountAsync(accountId: accountId);
            if (account != null) await _accountService.ActivateAsync(account.Id);
        }
    }
}
