using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public class FacultyTypeSeeder
    {
        public static async Task<List<FacultyType>> SeedAsync(UniPortalContext dbContext)
        {
            var now = DateTime.Now;
            var types = new List<FacultyType>
            {
                new FacultyType { Id = Guid.NewGuid(), Name = "Lecturer", CreatedAt = now },
                new FacultyType { Id = Guid.NewGuid(), Name = "Assistant Professor", CreatedAt = now },
                new FacultyType { Id = Guid.NewGuid(), Name = "Professor", CreatedAt = now }
            };

            foreach (var type in types)
            {
                if (!await dbContext.FacultyTypes.AnyAsync(ft => ft.Name == type.Name && !ft.IsDeleted))
                    dbContext.FacultyTypes.Add(type);
            }

            await dbContext.SaveChangesAsync();
            return types;
        }
    }
}
