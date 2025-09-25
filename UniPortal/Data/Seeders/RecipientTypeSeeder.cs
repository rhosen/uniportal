using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public class RecipientTypeSeeder
    {
        public static async Task SeedAsync(UniPortalContext dbContext)
        {
            var now = DateTime.Now;

            var recipients = new List<RecipientType>
            {
                new RecipientType { Id = Guid.NewGuid(), Name = Recipient.Student.ToString(), Description = "Notice for a single student", CreatedAt = now },
                new RecipientType { Id = Guid.NewGuid(), Name = Recipient.Faculty.ToString(), Description = "Notice for a single faculty", CreatedAt = now },
                new RecipientType { Id = Guid.NewGuid(), Name = Recipient.All.ToString(), Description = "Notice for everyone", CreatedAt = now }
            };

            foreach (var rec in recipients)
            {
                if (!await dbContext.RecipientTypes.AnyAsync(r => r.Name == rec.Name && !r.IsDeleted))
                    dbContext.RecipientTypes.Add(rec);
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
