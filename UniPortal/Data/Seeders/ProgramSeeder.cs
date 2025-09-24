using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public class ProgramSeeder
    {
        public static async Task<List<Data.Entities.Program>> SeedAsync(UniPortalContext dbContext, List<Department> departments, List<Degree> degrees)
        {
            var now = DateTime.Now;
            var programs = new List<Data.Entities.Program>
            {
                new Data.Entities.Program
                {
                    Id = Guid.NewGuid(),
                    Code = "BSC-CSE",
                    Name = "BSc in CSE",
                    DepartmentId = departments.First(d => d.Code == "CSE").Id,
                    DegreeId = degrees.First(d => d.Name == "Bachelor").Id,
                    TotalSemesters = 8,
                    Duration = 4,
                    TotalCreditsRequired = 132,
                    CreatedAt = now
                },
                new Data.Entities.Program
                {
                    Id = Guid.NewGuid(),
                    Code = "MSC-CSE",
                    Name = "MSc in CSE",
                    DepartmentId = departments.First(d => d.Code == "CSE").Id,
                    DegreeId = degrees.First(d => d.Name == "Master").Id,
                    TotalSemesters = 4,
                    Duration = 2,
                    TotalCreditsRequired = 36,
                    CreatedAt = now
                },
                new Data.Entities.Program
                {
                    Id = Guid.NewGuid(),
                    Code = "BBA-GEN",
                    Name = "BBA in General",
                    DepartmentId = departments.First(d => d.Code == "BBA").Id,
                    DegreeId = degrees.First(d => d.Name == "Bachelor").Id,
                    TotalSemesters = 8,
                    Duration = 4,
                    TotalCreditsRequired = 128,
                    CreatedAt = now
                }
            };

            foreach (var prog in programs)
            {
                if (!await dbContext.Programs.AnyAsync(p => p.Code == prog.Code && !p.IsDeleted))
                    dbContext.Programs.Add(prog);
            }

            await dbContext.SaveChangesAsync();
            return programs;
        }
    }
}
