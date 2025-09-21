using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public class AcademicSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var dbContext = services.GetRequiredService<UniPortalContext>();
            var now = DateTime.UtcNow;

            // --- Departments ---
            var cseDept = new Department { Id = Guid.NewGuid(), Code = "CSE", Name = "Computer Science & Engineering", CreatedAt = now, IsDeleted = false };
            var eeeDept = new Department { Id = Guid.NewGuid(), Code = "EEE", Name = "Electrical & Electronic Engineering", CreatedAt = now, IsDeleted = false };
            var bbaDept = new Department { Id = Guid.NewGuid(), Code = "BBA", Name = "Business Administration", CreatedAt = now, IsDeleted = false };
            dbContext.Departments.AddRange(cseDept, eeeDept, bbaDept);


            // --- Rooms ---
            var rooms = new[]
            {
                new Room { Id = Guid.NewGuid(), RoomName = "Room 101", Capacity = 40, Location = "Main Building - 1st Floor", CreatedAt = now, IsDeleted = false },
                new Room { Id = Guid.NewGuid(), RoomName = "Room 102", Capacity = 35, Location = "Main Building - 1st Floor", CreatedAt = now, IsDeleted = false },
                new Room { Id = Guid.NewGuid(), RoomName = "Lab 201",  Capacity = 25, Location = "Lab Building - 2nd Floor",  CreatedAt = now, IsDeleted = false },
                new Room { Id = Guid.NewGuid(), RoomName = "Conference Hall", Capacity = 60, Location = "Admin Block - Ground Floor", CreatedAt = now, IsDeleted = false }
            };
            dbContext.Rooms.AddRange(rooms);
            await dbContext.SaveChangesAsync();


            // --- Degrees ---
            var bachelorDegree = new Degree { Id = Guid.NewGuid(), Name = "Bachelor", CreatedAt = now, IsDeleted = false };
            var masterDegree = new Degree { Id = Guid.NewGuid(), Name = "Master", CreatedAt = now, IsDeleted = false };
            dbContext.Degrees.AddRange(bachelorDegree, masterDegree);

            await dbContext.SaveChangesAsync();

            // --- Programs ---
            var bscCse = new Data.Entities.Program
            {
                Id = Guid.NewGuid(),
                Code = "BSC-CSE",
                Name = "BSc in CSE",
                DepartmentId = cseDept.Id,
                DegreeId = bachelorDegree.Id,
                TotalSemesters = 8,
                Duration = 4,
                TotalCreditsRequired = 132,
                CreatedAt = now,
                IsDeleted = false
            };
            var mscCse = new Data.Entities.Program
            {
                Id = Guid.NewGuid(),
                Code = "MSC-CSE",
                Name = "MSc in CSE",
                DepartmentId = cseDept.Id,
                DegreeId = masterDegree.Id,
                TotalSemesters = 4,
                Duration = 2,
                TotalCreditsRequired = 36,
                CreatedAt = now,
                IsDeleted = false
            };
            var bbaGen = new Data.Entities.Program
            {
                Id = Guid.NewGuid(),
                Code = "BBA-GEN",
                Name = "BBA in General",
                DepartmentId = bbaDept.Id,
                DegreeId = bachelorDegree.Id,
                TotalSemesters = 8,
                Duration = 4,
                TotalCreditsRequired = 128,
                CreatedAt = now,
                IsDeleted = false
            };
            dbContext.Programs.AddRange(bscCse, mscCse, bbaGen);
            await dbContext.SaveChangesAsync();

            // --- Courses ---
            var math101 = new Course { Id = Guid.NewGuid(), Code = "MATH-101", Title = "Mathematics I", CreditHours = 3, DepartmentId = cseDept.Id, CreatedAt = now, IsDeleted = false };
            var eng101 = new Course { Id = Guid.NewGuid(), Code = "ENG-101", Title = "English Composition", CreditHours = 3, DepartmentId = cseDept.Id, CreatedAt = now, IsDeleted = false };
            var cse101 = new Course { Id = Guid.NewGuid(), Code = "CSE-101", Title = "Introduction to Programming", CreditHours = 4, DepartmentId = cseDept.Id, CreatedAt = now, IsDeleted = false };
            dbContext.Courses.AddRange(math101, eng101, cse101);
            await dbContext.SaveChangesAsync();

            // --- Course Types ---
            var coreType = new CourseType { Id = Guid.NewGuid(), Name = "Core", CreatedAt = now, IsDeleted = false };
            var generalType = new CourseType { Id = Guid.NewGuid(), Name = "General", CreatedAt = now, IsDeleted = false };
            dbContext.CourseTypes.AddRange(coreType, generalType);
            await dbContext.SaveChangesAsync();

            // --- Semesters ---
            var fall2025 = new Semester { Id = Guid.NewGuid(), SemesterType = "Fall", AcademicYear = "2025-2026", StartDate = new DateTime(2025, 8, 1), EndDate = new DateTime(2025, 12, 31), IsCurrent = false, CreatedAt = now, IsDeleted = false };
            var spring2025 = new Semester { Id = Guid.NewGuid(), SemesterType = "Spring", AcademicYear = "2025-2026", StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2025, 5, 31), IsCurrent = false, CreatedAt = now, IsDeleted = false };
            dbContext.Semesters.AddRange(fall2025, spring2025);
            await dbContext.SaveChangesAsync();

            // --- Batches ---
            var batch1 = new Batch { Id = Guid.NewGuid(), Number = "1", CreatedAt = now, IsDeleted = false };
            var batch2 = new Batch { Id = Guid.NewGuid(), Number = "2", CreatedAt = now, IsDeleted = false };
            var batch3 = new Batch { Id = Guid.NewGuid(), Number = "3", CreatedAt = now, IsDeleted = false };
            dbContext.Batches.AddRange(batch1, batch2, batch3);
            await dbContext.SaveChangesAsync();

            // --- Sections ---
            var sectionA = new Section { Id = Guid.NewGuid(), Name = "A", CreatedAt = now, IsDeleted = false };
            var sectionB = new Section { Id = Guid.NewGuid(), Name = "B", CreatedAt = now, IsDeleted = false };
            var sectionC = new Section { Id = Guid.NewGuid(), Name = "C", CreatedAt = now, IsDeleted = false };
            dbContext.Sections.AddRange(sectionA, sectionB, sectionC);
            await dbContext.SaveChangesAsync();

            // --- Curriculums ---
            var programs = new[] { bscCse, mscCse, bbaGen };
            var coursesList = new[] { math101, eng101, cse101 };
            var semestersList = new[] { fall2025, spring2025 };

            var curriculum = new List<Curriculum>();
            foreach (var program in programs)
            {
                foreach (var semester in semestersList)
                {
                    int semesterNumber = 1;
                    foreach (var course in coursesList)
                    {
                        var courseTypeToUse = course.DepartmentId == program.DepartmentId ? coreType : generalType;
                        curriculum.Add(new Curriculum
                        {
                            Id = Guid.NewGuid(),
                            ProgramId = program.Id,
                            SemesterId = semester.Id,
                            SemesterNumber = semesterNumber,
                            CourseId = course.Id,
                            CreditHours = course.CreditHours,
                            CourseTypeId = courseTypeToUse.Id,
                            SequenceOrder = 1,
                            CreatedAt = now,
                            IsDeleted = false
                        });
                    }
                }
            }
            dbContext.Curriculums.AddRange(curriculum);
            await dbContext.SaveChangesAsync();
        }
    }
}
