using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public class CurriculumSeeder
    {
        public static async Task SeedAsync(UniPortalContext dbContext, List<Data.Entities.Program> programs, List<Course> courses)
        {
            var now = DateTime.Now;

            foreach (var program in programs)
            {
                var programCourses = courses.Where(c =>
                    (program.Code == "BSC-CSE" && new[] { "MATH101", "PHYS101", "CSE101", "ENG101", "MATH102", "PHYS102", "CSE102", "COMM101" }.Contains(c.Code)) ||
                    (program.Code == "BBA-GEN" && new[] { "BUS101", "ACCT101", "ECON101", "COM101", "BUS102", "ACCT102", "ECON102", "LAW101" }.Contains(c.Code)) ||
                    (program.Code == "MSC-CSE" && new[] { "CSE501M", "CSE502M", "RES101", "CSE503M", "CSE504M", "CSE505M" }.Contains(c.Code))
                ).ToList();

                int semesterNumber = 1;
                int sequence = 1;
                int maxSemesters = 2;

                foreach (var course in programCourses)
                {
                    if (semesterNumber > maxSemesters) break;

                    if (!await dbContext.Curriculums.AnyAsync(c => c.ProgramId == program.Id && c.CourseId == course.Id))
                    {
                        dbContext.Curriculums.Add(new Curriculum
                        {
                            Id = Guid.NewGuid(),
                            ProgramId = program.Id,
                            SemesterNumber = semesterNumber,
                            CourseId = course.Id,
                            Sequence = sequence,
                            CreatedAt = now
                        });
                    }

                    sequence++;
                    int maxPerSemester = program.Code == "MSC-CSE" ? 3 : 4;
                    if (sequence > maxPerSemester)
                    {
                        sequence = 1;
                        semesterNumber++;
                    }
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
