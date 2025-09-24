using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public class SectionSeeder
    {
        public static async Task<List<Section>> SeedAsync(UniPortalContext dbContext)
        {
            var now = DateTime.Now;
            var sections = new List<Section>
            {
                new Section { Id = Guid.NewGuid(), Name = "A", CreatedAt = now },
                new Section { Id = Guid.NewGuid(), Name = "B", CreatedAt = now }
            };

            foreach (var section in sections)
            {
                if (!await dbContext.Sections.AnyAsync(s => s.Name == section.Name && !s.IsDeleted))
                    dbContext.Sections.Add(section);
            }

            await dbContext.SaveChangesAsync();
            return sections;
        }
    }
}
