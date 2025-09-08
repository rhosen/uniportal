using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public class RecipientTypeSeeder
    {
        public static async Task SeedRecipientTypesAsync(IServiceProvider services)
        {
            var dbContext = services.GetRequiredService<UniPortalContext>();

            // Check if already seeded
            if (await dbContext.RecipientTypes.AnyAsync())
                return;

            var recipients = new List<RecipientType>
            {
                new RecipientType
                {
                    Id = Guid.NewGuid(),
                    Name = "Student",
                    Description = "Notice for a single student",
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                },
                new RecipientType
                {
                    Id = Guid.NewGuid(),
                    Name = "Faculty",
                    Description = "Notice for all faculty members",
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                },
                new RecipientType
                {
                    Id = Guid.NewGuid(),
                    Name = "Department",
                    Description = "Notice for a whole department",
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                },
                new RecipientType
                {
                    Id = Guid.NewGuid(),
                    Name = "All",
                    Description = "Notice for everyone",
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            dbContext.RecipientTypes.AddRange(recipients);
            await dbContext.SaveChangesAsync();
        }
    }
}
