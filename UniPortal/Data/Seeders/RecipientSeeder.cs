using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public class RecipientSeeder
    {
        public static async Task SeedRecipientTypesAsync(IServiceProvider services)
        {
            var dbContext = services.GetRequiredService<UniPortalContext>();
            var now = DateTime.UtcNow;

            var recipients = new List<Recipient>
            {
                new Recipient
                {
                    Id = Guid.NewGuid(),
                    Name = "Student",
                    Description = "Notice for a single student",
                    IsDeleted = false,
                    CreatedAt = now
                },
                new Recipient
                {
                    Id = Guid.NewGuid(),
                    Name = "Faculty",
                    Description = "Notice for all faculty members",
                    IsDeleted = false,
                    CreatedAt = now
                },
                new Recipient
                {
                    Id = Guid.NewGuid(),
                    Name = "Department",
                    Description = "Notice for a whole department",
                    IsDeleted = false,
                    CreatedAt = now
                },
                new Recipient
                {
                    Id = Guid.NewGuid(),
                    Name = "All",
                    Description = "Notice for everyone",
                    IsDeleted = false,
                    CreatedAt = now
                }
            };

            dbContext.Recipients.AddRange(recipients);
            await dbContext.SaveChangesAsync();
        }
    }
}
