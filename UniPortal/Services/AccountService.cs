using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.ViewModel;

namespace UniPortal.Services
{
    public class AccountService : BaseService<Account>
    {
        private readonly UserService _userService;
        private readonly IUnitOfWork _unitOfWork;

        public AccountService(
            IUnitOfWork unitOfWork,
            LogService logService,
            UserService userService)
            : base(unitOfWork.Context, logService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
        }

        // Create account + IdentityUser atomically
        public async Task<Account> CreateAccountAsync(string email, string password, string role, string? firstName = null, string? lastName = null)
        {
            try
            {
                var identityUser = await _userService.RegisterUserAsync(email, password, role);

                var account = new Account
                {
                    IdentityUserId = identityUser.Id,
                    Email = email,
                    FirstName = firstName ?? "",
                    LastName = lastName ?? "",
                    IsActive = false,
                    IsDeleted = false
                };

                _unitOfWork.Context.Accounts.Add(account);

                await LogAsync(null, ActionType.Create, "Account", account.Id, new { Email = email });

                await _unitOfWork.CommitAsync();
                return account;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        // Flexible account retrieval
        public async Task<Account?> GetAccountAsync(Guid? accountId = null, string? identityUserId = null)
        {
            if (accountId.HasValue)
                return await _unitOfWork.Context.Accounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted);

            if (!string.IsNullOrEmpty(identityUserId))
                return await _unitOfWork.Context.Accounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.IdentityUserId == identityUserId && !a.IsDeleted);

            return null;
        }

        // Update profile
        public async Task<bool> UpdateProfileAsync(ProfileViewModel profile)
        {
            if (profile == null || profile.AccountId == Guid.Empty)
                return false;

            var account = await _unitOfWork.Context.Accounts.FirstOrDefaultAsync(a => a.Id == profile.AccountId && !a.IsDeleted);
            if (account == null) return false;

            account.FirstName = profile.FirstName;
            account.LastName = profile.LastName;
            account.Phone = profile.Phone;
            account.Address = profile.Address;
            account.DateOfBirth = profile.DateOfBirth;
            account.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Context.Accounts.Update(account);

            await LogAsync(account.Id, ActionType.Update, "Account", account.Id, profile);

            await _unitOfWork.CommitAsync();
            return true;
        }

        // Soft delete
        public async Task SoftDeleteAsync(Guid accountId)
        {
            var account = await _unitOfWork.Context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted);
            if (account == null) return;

            account.IsDeleted = true;
            account.DeletedAt = DateTime.UtcNow;

            _unitOfWork.Context.Accounts.Update(account);

            await LogAsync(account.Id, ActionType.Delete, "Account", account.Id);

            await _unitOfWork.CommitAsync();
        }

        // Activate account
        public async Task ActivateAsync(Guid accountId)
        {
            var account = await _unitOfWork.Context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted);
            if (account == null) return;

            account.IsActive = true;
            account.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Context.Accounts.Update(account);

            await LogAsync(account.Id, ActionType.Activate, "Account", account.Id);

            await _unitOfWork.CommitAsync();
        }

        // Update email atomically
        public async Task UpdateEmailAsync(Guid accountId, string newEmail)
        {
            try
            {
                var account = await _unitOfWork.Context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted);
                if (account == null) throw new Exception("Account not found");

                var identityUser = await _userService.GetUserByIdAsync(account.IdentityUserId);
                if (identityUser != null && identityUser.Email != newEmail)
                {
                    identityUser.Email = newEmail;
                    identityUser.UserName = newEmail;
                    await _userService.UpdateAsync(identityUser);
                }

                account.Email = newEmail;
                account.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Context.Accounts.Update(account);

                await LogAsync(account.Id, ActionType.Update, "Account", account.Id, new { newEmail });

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        // Password operations delegated to UserService
        public async Task<IdentityResult> UpdatePasswordAsync(Guid accountId, string newPassword)
        {
            var account = await _unitOfWork.Context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted);
            if (account == null) return IdentityResult.Failed(new IdentityError { Description = "Account not found" });

            var identityUser = await _userService.GetUserByIdAsync(account.IdentityUserId);
            if (identityUser == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            return await _userService.UpdatePasswordAsync(identityUser, newPassword);
        }

        public async Task<IdentityResult> ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword)
        {
            var account = await _unitOfWork.Context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted);
            if (account == null) return IdentityResult.Failed(new IdentityError { Description = "Account not found" });

            var identityUser = await _userService.GetUserByIdAsync(account.IdentityUserId);
            if (identityUser == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            return await _userService.ChangePasswordAsync(identityUser, currentPassword, newPassword);
        }

        // Get active students
        public async Task<List<Account>> GetActiveStudentsAsync()
        {
            var studentIds = (await _userService.GetUsersInRoleAsync("Student")).Select(u => u.Id).ToList();
            return await _unitOfWork.Context.Accounts
                .AsNoTracking()
                .Where(a => studentIds.Contains(a.IdentityUserId) && a.IsActive && !a.IsDeleted)
                .ToListAsync();
        }

        // Get inactive students
        public async Task<List<Account>> GetInactiveStudentsAsync()
        {
            var studentIds = (await _userService.GetUsersInRoleAsync("Student")).Select(u => u.Id).ToList();
            return await _unitOfWork.Context.Accounts
                .AsNoTracking()
                .Where(a => studentIds.Contains(a.IdentityUserId) && !a.IsActive && !a.IsDeleted)
                .ToListAsync();
        }
    }
}
