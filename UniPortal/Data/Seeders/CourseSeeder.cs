using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public class CourseSeeder
    {
        public static async Task<List<Course>> SeedAsync(UniPortalContext dbContext, List<Department> departments, List<CourseType> courseTypes)
        {
            var now = DateTime.Now;
            var courses = new List<Course>();

            var cseDept = departments.First(d => d.Code == "CSE");
            var bbaDept = departments.First(d => d.Code == "BBA");
            var coreType = courseTypes.First(t => t.Name == "Core");

            // BSc CSE Courses
            var bscCseCourses = new List<(string Code, string Title, int Credit)>
            {
                ("MATH101","Mathematics I",3),("PHYS101","Physics I",3),("CSE101","Introduction to Programming",4),("ENG101","English Composition",3),
                ("MATH102","Mathematics II",3),("PHYS102","Physics II",3),("CSE102","Data Structures",4),("COMM101","Communication Skills",3)
            };
            courses.AddRange(bscCseCourses.Select(c => new Course
            {
                Id = Guid.NewGuid(),
                Code = c.Code,
                Title = c.Title,
                CreditHours = c.Credit,
                DepartmentId = cseDept.Id,
                CourseTypeId = coreType.Id,
                CreatedAt = now
            }));

            // BBA Courses
            var bbaCourses = new List<(string Code, string Title, int Credit)>
            {
                ("BUS101","Principles of Management",3),("ACCT101","Accounting I",3),("ECON101","Economics I",3),("COM101","Business Communication",3),
                ("BUS102","Marketing Principles",3),("ACCT102","Accounting II",3),("ECON102","Economics II",3),("LAW101","Business Law",3)
            };
            courses.AddRange(bbaCourses.Select(c => new Course
            {
                Id = Guid.NewGuid(),
                Code = c.Code,
                Title = c.Title,
                CreditHours = c.Credit,
                DepartmentId = bbaDept.Id,
                CourseTypeId = coreType.Id,
                CreatedAt = now
            }));

            // MSc CSE Courses
            var mscCourses = new List<(string Code, string Title, int Credit)>
            {
                ("CSE501M","Advanced Algorithms",3), ("CSE502M","Advanced Database Systems",3), ("RES101","Research Methodology",3),
                ("CSE503M","AI & ML for MSc",3), ("CSE504M","Cloud Computing for MSc",3), ("CSE505M","Software Engineering for MSc",3)
            };
            courses.AddRange(mscCourses.Select(c => new Course
            {
                Id = Guid.NewGuid(),
                Code = c.Code,
                Title = c.Title,
                CreditHours = c.Credit,
                DepartmentId = cseDept.Id,
                CourseTypeId = coreType.Id,
                CreatedAt = now
            }));

            foreach (var course in courses)
            {
                if (!await dbContext.Courses.AnyAsync(c => c.Code == course.Code && !c.IsDeleted))
                    dbContext.Courses.Add(course);
            }

            await dbContext.SaveChangesAsync();
            return courses;
        }
    }
}
