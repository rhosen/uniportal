using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public class AcademicSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var dbContext = services.GetRequiredService<UniPortalContext>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

            // Seed academic structure
            var departments = await DepartmentSeeder.SeedAsync(dbContext);
            var degrees = await DegreeSeeder.SeedAsync(dbContext);
            var programs = await ProgramSeeder.SeedAsync(dbContext, departments, degrees);
            var courseTypes = await CourseTypeSeeder.SeedAsync(dbContext);
            var courses = await CourseSeeder.SeedAsync(dbContext, departments, courseTypes);
            var semesters = await SemesterSeeder.SeedAsync(dbContext);
            var batches = await BatchSeeder.SeedAsync(dbContext);
            var sections = await SectionSeeder.SeedAsync(dbContext);
            await CurriculumSeeder.SeedAsync(dbContext, programs, courses);

            // Seed faculty and faculty types
            var facultyTypes = await FacultyTypeSeeder.SeedAsync(dbContext);
            await FacultySeeder.SeedAsync(dbContext, userManager, departments, facultyTypes);

            // Seed students
            await StudentSeeder.SeedAsync(dbContext, userManager, programs, batches, sections);

            // Seed rooms (classrooms, labs, special rooms)
            await RoomSeeder.SeedAsync(dbContext);

            // Seed recipients (for notifications)
            await RecipientTypeSeeder.SeedAsync(dbContext);
        }
    }
}
