using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public class CourseTypeSeeder
    {
        public static async Task<List<CourseType>> SeedAsync(UniPortalContext dbContext)
        {
            var now = DateTime.Now;
            var types = new List<CourseType>
            {
                new CourseType { Id = Guid.NewGuid(), Name = "Core", CreatedAt = now },
                new CourseType { Id = Guid.NewGuid(), Name = "General", CreatedAt = now }
            };

            foreach (var type in types)
            {
                if (!await dbContext.CourseTypes.AnyAsync(t => t.Name == type.Name && !t.IsDeleted))
                    dbContext.CourseTypes.Add(type);
            }

            await dbContext.SaveChangesAsync();
            return types;
        }
    }
}
