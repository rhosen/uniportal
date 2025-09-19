using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public class AcademicSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var dbContext = services.GetRequiredService<UniPortalContext>();

            // --- Departments ---
            if (!await dbContext.Departments.AnyAsync())
            {
                var departments = new List<Department>
                {
                    new Department { Id = Guid.NewGuid(), Code = "CSE", Name = "Computer Science & Engineering", CreatedAt = DateTime.Now, IsDeleted = false },
                    new Department { Id = Guid.NewGuid(), Code = "EEE", Name = "Electrical & Electronic Engineering", CreatedAt = DateTime.Now, IsDeleted = false },
                    new Department { Id = Guid.NewGuid(), Code = "BBA", Name = "Business Administration", CreatedAt = DateTime.Now, IsDeleted = false }
                };
                dbContext.Departments.AddRange(departments);
            }

            // --- Subjects ---
            if (!await dbContext.Subjects.AnyAsync())
            {
                var subjects = new List<Subject>
                {
                    new Subject { Id = Guid.NewGuid(), Code = "MATH-101", Name = "Mathematics I", CreatedAt = DateTime.Now, IsDeleted = false },
                    new Subject { Id = Guid.NewGuid(), Code = "ENG-101", Name = "English Composition", CreatedAt = DateTime.Now, IsDeleted = false },
                    new Subject { Id = Guid.NewGuid(), Code = "CSE-101", Name = "Introduction to Programming", CreatedAt = DateTime.Now, IsDeleted = false }
                };
                dbContext.Subjects.AddRange(subjects);
            }

            // --- Rooms ---
            if (!await dbContext.Rooms.AnyAsync())
            {
                var rooms = new List<Room>
                {
                    new Room { Id = Guid.NewGuid(), RoomName = "215", Capacity = 40, Location = "2nd Floor", CreatedAt = DateTime.Now, IsDeleted = false },
                    new Room { Id = Guid.NewGuid(), RoomName = "301", Capacity = 60, Location = "3rd Floor", CreatedAt = DateTime.Now, IsDeleted = false }
                };
                dbContext.Rooms.AddRange(rooms);
            }

            // --- Semester ---
            if (!await dbContext.Semesters.AnyAsync())
            {
                var startDate = DateTime.Now.Date;
                var endDate = startDate.AddMonths(6);

                var semester = new Semester
                {
                    Id = Guid.NewGuid(),
                    Name = $"Fall {DateTime.Now.Year}",
                    StartDate = startDate,
                    EndDate = endDate,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };
                dbContext.Semesters.Add(semester);
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
