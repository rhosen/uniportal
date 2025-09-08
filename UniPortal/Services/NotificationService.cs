using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;

namespace UniPortal.Services
{
    public class NotificationService
    {
        private readonly UniPortalContext _context;

        public NotificationService(UniPortalContext context)
        {
            _context = context;
        }

        public async Task<List<Notification>> GetAllAsync()
        {
            return await _context.Notifications.Where(x=> !x.IsDeleted)
                .Include(n => n.CreatedByAccount)
                .Include(n => n.NotificationType)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification> GetByIdAsync(string id)
        {
            return await _context.Notifications
                .Include(n => n.CreatedByAccount)
                .Include(n => n.NotificationType)
                .FirstOrDefaultAsync(n => n.Id.ToString() == id);
        }

        public async Task CreateAsync(string title, string message, Guid createdBy, Guid notificationTypeId, string receiverId)
        {
            var notification = new Notification
            {
                Title = title,
                Message = message,
                CreatedById = createdBy,
                NotificationTypeId = notificationTypeId,
                ReceiverId = receiverId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, string title, string message, Guid notificationTypeId, string receiverId)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                notification.Title = title;
                notification.Message = message;
                notification.NotificationTypeId = notificationTypeId;
                notification.ReceiverId = receiverId;
                notification.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(string id)
        {
            var notification = await _context.Notifications.FindAsync(Guid.Parse(id));
            if (notification != null)
            {
                notification.IsDeleted = true;
                notification.DeletedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ActivateAsync(string id)
        {
            var notification = await _context.Notifications.FindAsync(Guid.Parse(id));
            if (notification != null)
            {
                notification.IsDeleted = false;
                notification.DeletedAt = null;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<NotificationType>> GetNotificationTypesAsync()
        {
            return await _context.NotificationTypes
                .Where(nt => !nt.IsDeleted)
                .OrderBy(nt => nt.Name)
                .ToListAsync();
        }

        public async Task<List<Account>> GetAccountsByRoleAsync(string roleName)
        {
            return await (from account in _context.Accounts
                          join userRole in _context.UserRoles on account.IdentityUserId equals userRole.UserId
                          join role in _context.Roles on userRole.RoleId equals role.Id
                          where role.Name == roleName && !account.IsDeleted && account.IsActive
                          select account)
                         .ToListAsync();
        }
        public async Task<List<Department>> GetDepartmentsAsync()
        {
            return await _context.Departments
                .Where(d => !d.IsDeleted)
                .ToListAsync();
        }
    }
}
