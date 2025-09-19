using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public class GradeScaleSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var dbContext = services.GetRequiredService<UniPortalContext>();

            if (!await dbContext.GradeScales.AnyAsync())
            {
                var gradeScales = new List<GradeScale>
                {
                    new GradeScale { Id = Guid.NewGuid(), Grade = "A+", MinMarks = 80, MaxMarks = 100, GPA = 4.00M, CreatedAt = DateTime.Now, IsDeleted = false },
                    new GradeScale { Id = Guid.NewGuid(), Grade = "A",  MinMarks = 75, MaxMarks = 79.99M, GPA = 3.75M, CreatedAt = DateTime.Now, IsDeleted = false },
                    new GradeScale { Id = Guid.NewGuid(), Grade = "A-", MinMarks = 70, MaxMarks = 74.99M, GPA = 3.50M, CreatedAt = DateTime.Now, IsDeleted = false },
                    new GradeScale { Id = Guid.NewGuid(), Grade = "B+", MinMarks = 65, MaxMarks = 69.99M, GPA = 3.25M, CreatedAt = DateTime.Now, IsDeleted = false },
                    new GradeScale { Id = Guid.NewGuid(), Grade = "B",  MinMarks = 60, MaxMarks = 64.99M, GPA = 3.00M, CreatedAt = DateTime.Now, IsDeleted = false },
                    new GradeScale { Id = Guid.NewGuid(), Grade = "B-", MinMarks = 55, MaxMarks = 59.99M, GPA = 2.75M, CreatedAt = DateTime.Now, IsDeleted = false },
                    new GradeScale { Id = Guid.NewGuid(), Grade = "C+", MinMarks = 50, MaxMarks = 54.99M, GPA = 2.50M, CreatedAt = DateTime.Now, IsDeleted = false },
                    new GradeScale { Id = Guid.NewGuid(), Grade = "C",  MinMarks = 45, MaxMarks = 49.99M, GPA = 2.25M, CreatedAt = DateTime.Now, IsDeleted = false },
                    new GradeScale { Id = Guid.NewGuid(), Grade = "D",  MinMarks = 40, MaxMarks = 44.99M, GPA = 2.00M, CreatedAt = DateTime.Now, IsDeleted = false },
                    new GradeScale { Id = Guid.NewGuid(), Grade = "F",  MinMarks = 0,  MaxMarks = 39.99M, GPA = 0.00M, CreatedAt = DateTime.Now, IsDeleted = false }
                };

                dbContext.GradeScales.AddRange(gradeScales);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
