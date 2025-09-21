using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;

namespace UniPortal.Services.Academics.Operations
{
    public class EnrollmentService
    {
        private readonly UniPortalContext _context;

        public EnrollmentService(UniPortalContext context)
        {
            _context = context;
        }

        // -----------------------------
        // Get students for a teacher in a semester
        // -----------------------------
        public async Task<List<SelectOption>> GetStudentsBySemesterAndTeacherAsync(int semesterNumber, Guid facultyId)
        {
            var students = await (
                from e in _context.Enrollments
                join co in _context.CourseOfferings on e.CourseOfferingId equals co.Id
                join s in _context.Students on e.StudentId equals s.Id
                join a in _context.Accounts on s.AccountId equals a.Id
                where co.SemesterNumber == semesterNumber
                      && co.FacultyId == facultyId
                      && !e.IsDeleted
                      && !s.IsDeleted
                select new
                {
                    s.Id,
                    s.StudentNumber,
                    a.FirstName,
                    a.LastName
                })
                .Distinct()
                .OrderBy(s => s.StudentNumber)
                .Select(s => new SelectOption
                {
                    Id = s.Id,
                    Name = $"{s.StudentNumber} - {s.FirstName} {s.LastName}"
                })
                .ToListAsync();

            return students;
        }


        // -----------------------------
        // Get enrollments for a student in a semester
        // -----------------------------
        public async Task<List<EnrollmentDto>> GetEnrollmentsAsync(Guid studentId, int semesterNumber)
        {
            var query = from e in _context.Enrollments
                        join co in _context.CourseOfferings on e.CourseOfferingId equals co.Id
                        join c in _context.Courses on co.CourseId equals c.Id
                        join t in _context.Accounts on co.FacultyId equals t.Id
                        where e.StudentId == studentId
                              && co.SemesterNumber == semesterNumber
                              && !e.IsDeleted
                        select new EnrollmentDto
                        {
                            Id = e.Id,
                            CourseId = co.Id,
                            CourseName = c.Code + " - " + c.Title,
                            TeacherName = t.FirstName + " " + t.LastName,
                            Credits = co.CreditHours
                        };

            return await query.ToListAsync();
        }


        // -----------------------------
        // Get past enrollments grouped by semester
        // -----------------------------
        public async Task<Dictionary<string, List<CourseDto>>> GetPastEnrollmentsGroupedBySemesterAsync(
     Guid studentId, int currentSemesterNumber)
        {
            var query = from e in _context.Enrollments
                        join co in _context.CourseOfferings on e.CourseOfferingId equals co.Id
                        join c in _context.Courses on co.CourseId equals c.Id
                        join t in _context.Accounts on co.FacultyId equals t.Id
                        join sem in _context.Semesters on co.SemesterId equals sem.Id
                        where e.StudentId == studentId
                              && co.SemesterNumber < currentSemesterNumber // use SemesterNumber
                              && !e.IsDeleted
                        orderby co.SemesterNumber // order by semester number
                        select new
                        {
                            SemesterName = $"{sem.SemesterType} ({sem.StartDate:MMM yyyy} - {sem.EndDate:MMM yyyy}) - Semester {co.SemesterNumber}",
                            Course = new CourseDto
                            {
                                Id = co.Id,
                                CourseName = c.Code + " - " + c.Title,
                                TeacherName = t.FirstName + " " + t.LastName,
                                Credits = co.CreditHours
                            }
                        };

            var list = await query.ToListAsync();

            return list
                .GroupBy(x => x.SemesterName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.Course).ToList()
                );
        }


        // -----------------------------
        // Update enrollments for a student
        // -----------------------------
        public async Task UpdateEnrollmentsAsync(Guid studentId, List<Guid> enrolledCourseOfferingIds, int semesterNumber, Guid modifiedBy)
        {
            // Fetch only the CourseOfferingIds for this student and semester
            var existingEnrollments = await _context.Enrollments
                .Where(e => e.StudentId == studentId && !e.IsDeleted)
                .Where(e => _context.CourseOfferings
                    .Where(co => co.SemesterNumber == semesterNumber)
                    .Select(co => co.Id)
                    .Contains(e.CourseOfferingId))
                .ToListAsync();

            var existingIds = existingEnrollments.Select(e => e.CourseOfferingId).ToList();

            // Courses to add
            var toAdd = enrolledCourseOfferingIds.Except(existingIds);
            foreach (var coId in toAdd)
            {
                _context.Enrollments.Add(new Enrollment
                {
                    Id = Guid.NewGuid(),
                    StudentId = studentId,
                    CourseOfferingId = coId,
                    ModifiedById = modifiedBy,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                });
            }

            // Courses to remove
            var toRemove = existingIds.Except(enrolledCourseOfferingIds);
            foreach (var coId in toRemove)
            {
                var enrollment = existingEnrollments.First(e => e.CourseOfferingId == coId);
                enrollment.IsDeleted = true;
                enrollment.ModifiedById = modifiedBy;
                enrollment.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }


    }
}
