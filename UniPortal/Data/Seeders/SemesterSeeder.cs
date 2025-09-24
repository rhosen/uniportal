using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public class SemesterSeeder
    {
        public static async Task<List<Semester>> SeedAsync(UniPortalContext dbContext)
        {
            var now = DateTime.Now;
            var currentDate = now.Date;

            var semesters = new List<Semester>
            {
                new Semester
                {
                    Id = Guid.NewGuid(),
                    SemesterType = "Spring",
                    AcademicYear = $"{currentDate.Year}-{currentDate.Year+1}",
                    StartDate = new DateTime(currentDate.Year,1,1),
                    EndDate = new DateTime(currentDate.Year,6,30),
                    IsCurrent = currentDate.Month <= 6,
                    CreatedAt = now
                },
                new Semester
                {
                    Id = Guid.NewGuid(),
                    SemesterType = "Fall",
                    AcademicYear = $"{currentDate.Year}-{currentDate.Year+1}",
                    StartDate = new DateTime(currentDate.Year,7,1),
                    EndDate = new DateTime(currentDate.Year,12,31),
                    IsCurrent = currentDate.Month >= 7,
                    CreatedAt = now
                }
            };

            foreach (var semester in semesters)
            {
                if (!await dbContext.Semesters.AnyAsync(s => s.SemesterType == semester.SemesterType && s.AcademicYear == semester.AcademicYear && !s.IsDeleted))
                    dbContext.Semesters.Add(semester);
            }

            await dbContext.SaveChangesAsync();
            return semesters;
        }
    }
}
