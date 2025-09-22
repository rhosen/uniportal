using Microsoft.AspNetCore.Identity;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using static UniPortal.Constants.AppConstant;

namespace UniPortal.Data.Seeders
{
    public class AcademicSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var dbContext = services.GetRequiredService<UniPortalContext>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var now = DateTime.Now;

            // --- Departments ---
            var cseDept = new Department { Id = Guid.NewGuid(), Code = "CSE", Name = "Computer Science & Engineering", CreatedAt = now, IsDeleted = false };
            var bbaDept = new Department { Id = Guid.NewGuid(), Code = "BBA", Name = "Business Administration", CreatedAt = now, IsDeleted = false };
            dbContext.Departments.AddRange(cseDept, bbaDept);
            await dbContext.SaveChangesAsync();

            // --- Rooms ---
            var rooms = new List<Room>();
            for (int i = 101; i <= 115; i++)
                rooms.Add(new Room { Id = Guid.NewGuid(), RoomName = $"Room {i}", Capacity = 25 + (i % 5) * 5, Location = "Main Building", CreatedAt = now, IsDeleted = false });
            for (int i = 201; i <= 215; i++)
                rooms.Add(new Room { Id = Guid.NewGuid(), RoomName = $"Lab {i}", Capacity = 20 + (i % 5) * 5, Location = "Lab Building", CreatedAt = now, IsDeleted = false });
            var specialRooms = new[] { "Conference Hall", "Auditorium", "Seminar Room", "Workshop Room" };
            rooms.AddRange(specialRooms.Select(r => new Room { Id = Guid.NewGuid(), RoomName = r, Capacity = r == "Auditorium" ? 100 : 60, Location = "Admin Block", CreatedAt = now, IsDeleted = false }));
            dbContext.Rooms.AddRange(rooms);
            await dbContext.SaveChangesAsync();

            // --- Degrees ---
            var bachelorDegree = new Degree { Id = Guid.NewGuid(), Name = "Bachelor", CreatedAt = now, IsDeleted = false };
            var masterDegree = new Degree { Id = Guid.NewGuid(), Name = "Master", CreatedAt = now, IsDeleted = false };
            dbContext.Degrees.AddRange(bachelorDegree, masterDegree);
            await dbContext.SaveChangesAsync();

            // --- Programs ---
            var programs = new[]
            {
                new Data.Entities.Program { Id = Guid.NewGuid(), Code = "BSC-CSE", Name = "BSc in CSE", DepartmentId = cseDept.Id, DegreeId = bachelorDegree.Id, TotalSemesters = 8, Duration = 4, TotalCreditsRequired = 132, CreatedAt = now, IsDeleted = false },
                new Data.Entities.Program { Id = Guid.NewGuid(), Code = "MSC-CSE", Name = "MSc in CSE", DepartmentId = cseDept.Id, DegreeId = masterDegree.Id, TotalSemesters = 4, Duration = 2, TotalCreditsRequired = 36, CreatedAt = now, IsDeleted = false },
                new Data.Entities.Program { Id = Guid.NewGuid(), Code = "BBA-GEN", Name = "BBA in General", DepartmentId = bbaDept.Id, DegreeId = bachelorDegree.Id, TotalSemesters = 8, Duration = 4, TotalCreditsRequired = 128, CreatedAt = now, IsDeleted = false }
            };
            dbContext.Programs.AddRange(programs);
            await dbContext.SaveChangesAsync();

            // --- Course Types ---
            var coreType = new CourseType { Id = Guid.NewGuid(), Name = "Core", CreatedAt = now, IsDeleted = false };
            var generalType = new CourseType { Id = Guid.NewGuid(), Name = "General", CreatedAt = now, IsDeleted = false };
            dbContext.CourseTypes.AddRange(coreType, generalType);
            await dbContext.SaveChangesAsync();

            // --- Courses ---
            var courses = new List<Course>();

            // BSc CSE Courses
            var bscCseCourses = new List<(string Code, string Title, int Credit)>
            {
                ("MATH101","Mathematics I",3),("PHYS101","Physics I",3),("CSE101","Introduction to Programming",4),("ENG101","English Composition",3),
                ("MATH102","Mathematics II",3),("PHYS102","Physics II",3),("CSE102","Data Structures",4),("COMM101","Communication Skills",3)
            };
            courses.AddRange(bscCseCourses.Select(c => new Course { Id = Guid.NewGuid(), Code = c.Code, Title = c.Title, CreditHours = c.Credit, DepartmentId = cseDept.Id, CourseTypeId = coreType.Id, CreatedAt = now, IsDeleted = false }));

            // BBA Courses
            var bbaCourses = new List<(string Code, string Title, int Credit)>
            {
                ("BUS101","Principles of Management",3),("ACCT101","Accounting I",3),("ECON101","Economics I",3),("COM101","Business Communication",3),
                ("BUS102","Marketing Principles",3),("ACCT102","Accounting II",3),("ECON102","Economics II",3),("LAW101","Business Law",3)
            };
            courses.AddRange(bbaCourses.Select(c => new Course { Id = Guid.NewGuid(), Code = c.Code, Title = c.Title, CreditHours = c.Credit, DepartmentId = bbaDept.Id, CourseTypeId = coreType.Id, CreatedAt = now, IsDeleted = false }));

            // MSc CSE Courses
            var mscCourses = new List<(string Code, string Title, int Credit)>
            {
                ("CSE501M","Advanced Algorithms",3), ("CSE502M","Advanced Database Systems",3), ("RES101","Research Methodology",3),
                ("CSE503M","AI & ML for MSc",3), ("CSE504M","Cloud Computing for MSc",3), ("CSE505M","Software Engineering for MSc",3)
            };
            courses.AddRange(mscCourses.Select(c => new Course { Id = Guid.NewGuid(), Code = c.Code, Title = c.Title, CreditHours = c.Credit, DepartmentId = cseDept.Id, CourseTypeId = coreType.Id, CreatedAt = now, IsDeleted = false }));

            dbContext.Courses.AddRange(courses);
            await dbContext.SaveChangesAsync();

            // --- Semesters ---
            var currentDate = DateTime.Now.Date;
            var semesters = new List<Semester>
            {
                new Semester { Id = Guid.NewGuid(), SemesterType = "Spring", AcademicYear = $"{currentDate.Year}-{currentDate.Year+1}", StartDate = new DateTime(currentDate.Year,1,1), EndDate = new DateTime(currentDate.Year,6,30), IsCurrent = currentDate.Month <= 6, CreatedAt = now, IsDeleted = false },
                new Semester { Id = Guid.NewGuid(), SemesterType = "Fall", AcademicYear = $"{currentDate.Year}-{currentDate.Year+1}", StartDate = new DateTime(currentDate.Year,7,1), EndDate = new DateTime(currentDate.Year,12,31), IsCurrent = currentDate.Month >= 7, CreatedAt = now, IsDeleted = false }
            };
            dbContext.Semesters.AddRange(semesters);
            await dbContext.SaveChangesAsync();

            // --- Batches ---
            var batches = new List<Batch>
            {
                new Batch { Id = Guid.NewGuid(), Number = "1", CreatedAt = now, IsDeleted = false },
                new Batch { Id = Guid.NewGuid(), Number = "2", CreatedAt = now, IsDeleted = false }
            };
            dbContext.Batches.AddRange(batches);
            await dbContext.SaveChangesAsync();

            // --- Sections ---
            var sections = new List<Section>
            {
                new Section { Id = Guid.NewGuid(), Name = "A", CreatedAt = now, IsDeleted = false },
                new Section { Id = Guid.NewGuid(), Name = "B", CreatedAt = now, IsDeleted = false }
            };
            dbContext.Sections.AddRange(sections);
            await dbContext.SaveChangesAsync();

            // --- Curriculum Mapping (first 2 semesters) ---
            foreach (var program in programs)
            {
                var programCourses = courses.Where(c =>
                    (program.Code == "BSC-CSE" && bscCseCourses.Any(b => b.Code == c.Code)) ||
                    (program.Code == "BBA-GEN" && bbaCourses.Any(b => b.Code == c.Code)) ||
                    (program.Code == "MSC-CSE" && mscCourses.Any(m => m.Code == c.Code))
                ).ToList();

                int semesterNumber = 1;
                int sequenceOrder = 1;
                int maxSemesters = 2;

                foreach (var course in programCourses)
                {
                    if (semesterNumber > maxSemesters) break;

                    dbContext.Curriculums.Add(new Curriculum
                    {
                        Id = Guid.NewGuid(),
                        ProgramId = program.Id,
                        SemesterNumber = semesterNumber,
                        CourseId = course.Id,
                        SequenceOrder = sequenceOrder,
                        CreatedAt = now,
                        IsDeleted = false
                    });

                    sequenceOrder++;
                    int maxPerSemester = program.Code == "MSC-CSE" ? 3 : 4;
                    if (sequenceOrder > maxPerSemester)
                    {
                        sequenceOrder = 1;
                        semesterNumber++;
                    }
                }
            }
            await dbContext.SaveChangesAsync();

            // --- Roles ---
            if (!await roleManager.RoleExistsAsync(Roles.Faculty))
                await roleManager.CreateAsync(new IdentityRole(Roles.Faculty));
            if (!await roleManager.RoleExistsAsync(Roles.Student))
                await roleManager.CreateAsync(new IdentityRole(Roles.Student));

            // --- Faculties (4 per department, hard-coded authentic) ---
            var facultyData = new Dictionary<Department, List<(string FirstName, string LastName, string Email, string FacultyNumber)>>()
            {
                [cseDept] = new List<(string, string, string, string)>
                {
                    ("John","Doe","john.doe@uniportal.com","CSEFAC001"),
                    ("Alice","Smith","alice.smith@uniportal.com","CSEFAC002"),
                    ("Robert","Brown","robert.brown@uniportal.com","CSEFAC003"),
                    ("Emma","Wilson","emma.wilson@uniportal.com","CSEFAC004")
                },
                [bbaDept] = new List<(string, string, string, string)>
                {
                    ("Michael","Johnson","michael.johnson@uniportal.com","BBAFAC001"),
                    ("Sarah","Davis","sarah.davis@uniportal.com","BBAFAC002"),
                    ("David","Miller","david.miller@uniportal.com","BBAFAC003"),
                    ("Emily","Taylor","emily.taylor@uniportal.com","BBAFAC004")
                }
            };

            foreach (var kvp in facultyData)
            {
                var dept = kvp.Key;
                foreach (var (firstName, lastName, email, facultyNumber) in kvp.Value)
                {
                    if (dbContext.Accounts.Any(a => a.Email == email)) continue;

                    var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                    await userManager.CreateAsync(user, Passwords.Faculty);
                    await userManager.AddToRoleAsync(user, Roles.Faculty);

                    var account = new Account
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,
                        IdentityId = user.Id,
                        IsActive = true,
                        Gender = "Other",
                        CreatedAt = now
                    };
                    dbContext.Accounts.Add(account);

                    var faculty = new Faculty
                    {
                        AccountId = account.Id,
                        FacultyNumber = facultyNumber,
                        DepartmentId = dept.Id,
                        CreatedAt = now
                    };
                    dbContext.Faculties.Add(faculty);
                }
            }
            await dbContext.SaveChangesAsync();

            // --- Students (distinct, hard-coded) ---
            var studentData = new List<(string FirstName, string LastName, string StudentNumber)>
            {
                ("Liam","Anderson","STU001"),
                ("Olivia","Thomas","STU002"),
                ("Noah","Martin","STU003"),
                ("Emma","Lee","STU004")
            };

            var defaultProgram = programs.First();
            var defaultBatch = batches.First();
            var defaultSection = sections.First();

            foreach (var (firstName, lastName, studentNumber) in studentData)
            {
                var email = $"{firstName.ToLower()}.{lastName.ToLower()}@uniportal.com";
                if (dbContext.Accounts.Any(a => a.Email == email)) continue;

                var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                await userManager.CreateAsync(user, Passwords.Student);
                await userManager.AddToRoleAsync(user, Roles.Student);

                var account = new Account
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    IdentityId = user.Id,
                    IsActive = true,
                    Gender = "Other",
                    CreatedAt = now
                };
                dbContext.Accounts.Add(account);

                var student = new Student
                {
                    AccountId = account.Id,
                    StudentNumber = studentNumber,
                    ProgramId = defaultProgram.Id,
                    BatchId = defaultBatch.Id,
                    SectionId = defaultSection.Id,
                    CurrentSemester = 1,
                    CreatedAt = now
                };
                dbContext.Students.Add(student);
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
