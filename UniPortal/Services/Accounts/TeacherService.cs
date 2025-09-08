using UniPortal.Data.Entities;
using UniPortal.ViewModels.Users;
using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;

namespace UniPortal.Services.Accounts
{
    public class TeacherService
    {
        private readonly AccountService _accountService;
        private readonly UniPortalContext _context;

        public TeacherService(AccountService accountService, UniPortalContext context)
        {
            _accountService = accountService;
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Get all active teachers
        public async Task<List<Account>> GetAllAsync()
        {
            var facultyRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == Roles.Faculty);
            if (facultyRole == null) return new List<Account>();

            var facultyUserIds = await _context.UserRoles
                .Where(ur => ur.RoleId == facultyRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var teachers = await _context.Accounts
                .Where(a => !a.IsDeleted && a.IsActive && facultyUserIds.Contains(a.IdentityUserId))
                .ToListAsync();

            return teachers;
        }

        // Get teacher by AccountId
        public async Task<Account> GetByIdAsync(Guid accountId)
        {
            return await _accountService.GetAccountAsync(accountId: accountId);
        }

        // Create new teacher
        public async Task CreateAsync(string email, string password, string firstName, string lastName)
        {
            await _accountService.CreateAccountAsync(email, password, Roles.Faculty, firstName, lastName);
        }

        // Update teacher info (full profile)
        public async Task<bool> UpdateAsync(AccountViewModel profile)
        {
            var account = await _accountService.GetAccountAsync(accountId: profile.AccountId);
            if (account == null) return false;

            // Update email if changed
            if (account.Email != profile.Email)
                await _accountService.UpdateEmailAsync(profile.AccountId, profile.Email);

            // Update full profile via AccountService
            return await _accountService.UpdateProfileAsync(profile);
        }

        // Soft delete teacher
        public async Task DeleteAsync(Guid accountId)
        {
            await _accountService.SoftDeleteAsync(accountId);
        }

        // Activate teacher
        public async Task ActivateAsync(Guid accountId)
        {
            var account = await _accountService.GetAccountAsync(accountId: accountId);
            if (account != null) await _accountService.ActivateAsync(account.Id);
        }
    }
}
