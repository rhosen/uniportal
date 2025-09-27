using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Dtos;
using UniPortal.Data.Entities;

namespace UniPortal.Services.Academics.Portals
{
    public class InboxService
    {
        private readonly UniPortalContext _context;

        public InboxService(UniPortalContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get notifications for a specific user (student or faculty)
        /// including read status.
        /// </summary>
        public async Task<List<NotificationDto>> GetUserNotificationsAsync(Guid accountId, string role, int page = 1, int pageSize = 20)
        {
            var skip = (page - 1) * pageSize;
            var roleId = GetRecipientTypeId(role);
            var allId = GetRecipientTypeId("All");

            var query = _context.Notifications
                .AsNoTracking()
                .Where(n => !n.IsDeleted &&
                            (n.RecipientTypeId == roleId || n.RecipientTypeId == allId || n.AccountId == accountId))
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    FilePath = n.FilePath,
                    RecipientTypeId = n.RecipientTypeId,
                    AccountId = n.AccountId,
                    CreatedAt = n.CreatedAt,
                    // Check read status per user
                    IsRead = _context.NotificationReads
                                .Any(r => r.NotificationId == n.Id && r.AccountId == accountId && r.IsRead)
                });

            return await query.ToListAsync();
        }

        /// <summary>
        /// Get a single notification by Id for display or redirect.
        /// </summary>
        public async Task<NotificationDto> GetNotificationByIdAsync(Guid notificationId, Guid accountId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.Id == notificationId && !n.IsDeleted)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    FilePath = n.FilePath,
                    RecipientTypeId = n.RecipientTypeId,
                    AccountId = n.AccountId,
                    CreatedAt = n.CreatedAt,
                    IsRead = _context.NotificationReads
                                .Any(r => r.NotificationId == n.Id && r.AccountId == accountId && r.IsRead)
                })
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Mark a notification as read for a specific user and update ReadAt.
        /// </summary>
        public async Task MarkAsReadAsync(Guid notificationId, Guid accountId)
        {
            var readEntry = await _context.NotificationReads
                .FirstOrDefaultAsync(r => r.NotificationId == notificationId && r.AccountId == accountId);

            if (readEntry != null)
            {
                if (!readEntry.IsRead)
                {
                    readEntry.IsRead = true;
                    readEntry.ReadAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                _context.NotificationReads.Add(new NotificationRead
                {
                    Id = Guid.NewGuid(),
                    NotificationId = notificationId,
                    AccountId = accountId,
                    IsRead = true,
                    ReadAt = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Count total notifications for pagination.
        /// </summary>
        public async Task<int> GetTotalNotificationsAsync(Guid accountId, string role)
        {
            var roleId = GetRecipientTypeId(role);
            var allId = GetRecipientTypeId("All");

            return await _context.Notifications
                .AsNoTracking()
                .CountAsync(n => !n.IsDeleted &&
                                 (n.RecipientTypeId == roleId || n.RecipientTypeId == allId || n.AccountId == accountId));
        }

        /// <summary>
        /// Maps role name to recipient type GUID.
        /// </summary>
        private Guid GetRecipientTypeId(string roleOrType)
        {
            var recipient = _context.RecipientTypes
                .AsNoTracking()
                .FirstOrDefault(r => r.Name.ToLower() == roleOrType.ToLower() && !r.IsDeleted);

            if (recipient == null)
                throw new InvalidOperationException($"Recipient type not found: {roleOrType}");

            return recipient.Id;
        }
    }
}
