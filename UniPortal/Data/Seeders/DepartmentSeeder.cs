using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public class DepartmentSeeder
    {
        public static async Task<List<Department>> SeedAsync(UniPortalContext dbContext)
        {
            var now = DateTime.Now;

            var departments = new List<Department>
            {
                new Department { Id = Guid.NewGuid(), Code = "CSE", Name = "Computer Science & Engineering", CreatedAt = now },
                new Department { Id = Guid.NewGuid(), Code = "BBA", Name = "Business Administration", CreatedAt = now }
            };

            foreach (var dept in departments)
            {
                if (!await dbContext.Departments.AnyAsync(d => d.Code == dept.Code && !d.IsDeleted))
                    dbContext.Departments.Add(dept);
            }

            await dbContext.SaveChangesAsync();
            return departments;
        }
    }
}
