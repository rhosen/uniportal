using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public class BatchSeeder
    {
        public static async Task<List<Batch>> SeedAsync(UniPortalContext dbContext)
        {
            var now = DateTime.Now;
            var batches = new List<Batch>
            {
                new Batch { Id = Guid.NewGuid(), Name = "1", CreatedAt = now },
                new Batch { Id = Guid.NewGuid(), Name = "2", CreatedAt = now }
            };

            foreach (var batch in batches)
            {
                if (!await dbContext.Batches.AnyAsync(b => b.Name == batch.Name && !b.IsDeleted))
                    dbContext.Batches.Add(batch);
            }

            await dbContext.SaveChangesAsync();
            return batches;
        }
    }
}
