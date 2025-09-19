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
                await dbContext.SaveChangesAsync(); // save to get Department Ids
            }

            // --- Programs ---
            if (!await dbContext.Programs.AnyAsync())
            {
                var cseDept = await dbContext.Departments.FirstAsync(d => d.Code == "CSE");
                var programs = new List<Entities.Program>
                {
                    new Entities.Program { Id = Guid.NewGuid(), Code = "BSC-CSE", Name = "BSc in CSE", DepartmentId = cseDept.Id, CreatedAt = DateTime.Now, IsDeleted = false }
                };
                dbContext.Programs.AddRange(programs);
                await dbContext.SaveChangesAsync(); // save to get Program Ids
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
                await dbContext.SaveChangesAsync();
            }

            // --- Rooms ---
            if (!await dbContext.Rooms.AnyAsync())
            {
                var rooms = new List<Room>
                {
                    new Room { Id = Guid.NewGuid(), RoomName = "215", Capacity = 40, Location = "2nd Floor", CreatedAt = DateTime.Now, IsDeleted = false },
                    new Room { Id = Guid.NewGuid(), RoomName = "301", Capacity = 60, Location = "3rd Floor", CreatedAt = DateTime.Now, IsDeleted = false },
                    new Room { Id = Guid.NewGuid(), RoomName = "101", Capacity = 60, Location = "1st Floor", CreatedAt = DateTime.Now, IsDeleted = false }
                };
                dbContext.Rooms.AddRange(rooms);
                await dbContext.SaveChangesAsync();
            }

            // --- Semesters per Program ---
            if (!await dbContext.Semesters.AnyAsync())
            {
                var startYear = DateTime.Now.Year;

                var programs = await dbContext.Programs.ToListAsync();

                var semesters = new List<Semester>();

                foreach (var program in programs)
                {
                    for (int i = 1; i <= 8; i++) // assuming 8 semesters per program
                    {
                        int yearOffset = (i - 1) / 2; // 2 semesters per year
                        int semesterMonthStart = ((i - 1) % 2) * 6 + 1;
                        int semesterMonthEnd = semesterMonthStart + 5; // 6-month semester

                        semesters.Add(new Semester
                        {
                            Id = Guid.NewGuid(),
                            Name = $"{program.Code} – Semester {i}",
                            ProgramId = program.Id,
                            StartDate = new DateTime(startYear + yearOffset, semesterMonthStart, 1),
                            EndDate = new DateTime(startYear + yearOffset, semesterMonthEnd,
                                DateTime.DaysInMonth(startYear + yearOffset, semesterMonthEnd)),
                            CreatedAt = DateTime.Now,
                            IsDeleted = false
                        });
                    }
                }

                dbContext.Semesters.AddRange(semesters);
                await dbContext.SaveChangesAsync();
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
