using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public class DegreeSeeder
    {
        public static async Task<List<Degree>> SeedAsync(UniPortalContext dbContext)
        {
            var now = DateTime.Now;
            var degrees = new List<Degree>
            {
                new Degree { Id = Guid.NewGuid(), Name = "Bachelor", CreatedAt = now },
                new Degree { Id = Guid.NewGuid(), Name = "Master", CreatedAt = now }
            };

            foreach (var deg in degrees)
            {
                if (!await dbContext.Degrees.AnyAsync(d => d.Name == deg.Name && !d.IsDeleted))
                    dbContext.Degrees.Add(deg);
            }

            await dbContext.SaveChangesAsync();
            return degrees;
        }
    }
}
