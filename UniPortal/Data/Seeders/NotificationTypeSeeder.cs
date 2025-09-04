using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public class NotificationTypeSeeder
    {
        public static async Task SeedNotificationTypesAsync(IServiceProvider services)
        {
            var dbContext = services.GetRequiredService<UniPortalContext>();

            // Check if already seeded
            if (await dbContext.NotificationTypes.AnyAsync())
                return;

            var types = new List<NotificationType>
            {
                new NotificationType
                {
                    Id = Guid.NewGuid(),
                    Name = "Student",
                    Description = "Notification for a single student",
                    IsDeleted = false,
                    CreatedAt = DateTime.Now
                },
                new NotificationType
                {
                    Id = Guid.NewGuid(),
                    Name = "Faculty",
                    Description = "Notification for all faculty members",
                    IsDeleted = false,
                    CreatedAt = DateTime.Now
                },
                new NotificationType
                {
                    Id = Guid.NewGuid(),
                    Name = "Department",
                    Description = "Notification for a whole department",
                    IsDeleted = false,
                    CreatedAt = DateTime.Now
                },
                new NotificationType
                {
                    Id = Guid.NewGuid(),
                    Name = "All",
                    Description = "Notification for everyone",
                    IsDeleted = false,
                    CreatedAt = DateTime.Now
                }
            };

            dbContext.NotificationTypes.AddRange(types);
            await dbContext.SaveChangesAsync();
        }
    }
}
