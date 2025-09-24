using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public class RecipientSeeder
    {
        public static async Task SeedAsync(UniPortalContext dbContext)
        {
            var now = DateTime.Now;

            var recipients = new List<Recipient>
            {
                new Recipient { Id = Guid.NewGuid(), Name = "Student", Description = "Notice for a single student", CreatedAt = now },
                new Recipient { Id = Guid.NewGuid(), Name = "Faculty", Description = "Notice for a single faculty", CreatedAt = now },
                new Recipient { Id = Guid.NewGuid(), Name = "All", Description = "Notice for everyone", CreatedAt = now }
            };

            foreach (var rec in recipients)
            {
                if (!await dbContext.Recipients.AnyAsync(r => r.Name == rec.Name && !r.IsDeleted))
                    dbContext.Recipients.Add(rec);
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
