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

        public async Task<List<SelectOption>> GetStudentsBySemesterAndTeacherAsync(Guid semesterId, Guid teacherId)
        {
            // Join Enrollment → Student → Account → Course in a single query
            var students = await _context.Enrollments
                .AsNoTracking()
                .Where(e => e.Course.TeacherId == teacherId && e.Course.SemesterId == semesterId)
                .Select(e => new
                {
                    e.Student.Id,               // Student table PK
                    e.Student.StudentId,        // Human-readable student ID
                    e.Student.Account.FirstName,
                    e.Student.Account.LastName
                })
                .Distinct() // Avoid duplicate students enrolled in multiple courses
                .OrderBy(s => s.StudentId)
                .Select(s => new SelectOption
                {
                    Id = s.Id,
                    Name = $"{s.StudentId} - {s.FirstName} {s.LastName}"
                })
                .ToListAsync();

            return students;
        }

        public async Task<List<EnrollmentDto>> GetEnrollmentsAsync(Guid studentId, Guid semesterId)
        {
            var query = from e in _context.Enrollments
                        join c in _context.Courses on e.CourseId equals c.Id
                        join s in _context.Subjects on c.SubjectId equals s.Id
                        join t in _context.Accounts on c.TeacherId equals t.Id
                        where e.StudentId == studentId && c.SemesterId == semesterId && !e.IsDeleted
                        select new EnrollmentDto
                        {
                            Id = e.Id,
                            CourseId = c.Id,
                            CourseName = s.Code + " - " + s.Name,
                            TeacherName = t.FirstName + " " + t.LastName,
                            Credits = c.Credits
                        };

            return await query.ToListAsync();
        }

        public async Task<Dictionary<string, List<CourseDto>>> GetPastEnrollmentsGroupedBySemesterAsync(Guid studentId, Guid currentSemesterId)
        {
            var query = from e in _context.Enrollments
                        join c in _context.Courses on e.CourseId equals c.Id
                        join s in _context.Subjects on c.SubjectId equals s.Id
                        join t in _context.Accounts on c.TeacherId equals t.Id
                        join sem in _context.Semesters on c.SemesterId equals sem.Id
                        where e.StudentId == studentId
                              && c.SemesterId != currentSemesterId
                              && !e.IsDeleted
                        orderby sem.StartDate
                        select new
                        {
                            SemesterName = sem.Name + " (" + sem.StartDate.ToString("MMM yyyy") + " - " + sem.EndDate.ToString("MMM yyyy") + ")",
                            Course = new CourseDto
                            {
                                Id = c.Id,
                                CourseName = s.Code + " - " + s.Name,
                                TeacherName = t.FirstName + " " + t.LastName,
                                Credits = c.Credits
                            }
                        };

            var list = await query.ToListAsync();

            // Group by semester name
            var grouped = list
                .GroupBy(x => x.SemesterName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.Course).ToList()
                );

            return grouped;
        }

        public async Task UpdateEnrollmentsAsync(Guid studentId, List<Guid> enrolledCourseIds, Guid currentSemesterId, Guid modifiedBy)
        {
            // Fetch existing enrollments for this student and semester
            var existingEnrollments = await _context.Enrollments
                .Where(e => e.StudentId == studentId && e.Course.SemesterId == currentSemesterId && !e.IsDeleted)
                .ToListAsync();

            var existingCourseIds = existingEnrollments.Select(e => e.CourseId).ToList();

            // Courses to add (newly toggled ON)
            var coursesToAdd = enrolledCourseIds.Except(existingCourseIds).ToList();
            foreach (var courseId in coursesToAdd)
            {
                var enrollment = new Enrollment
                {
                    Id = Guid.NewGuid(),
                    StudentId = studentId,
                    CourseId = courseId,
                    ModifiedById = modifiedBy,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };
                _context.Enrollments.Add(enrollment);
            }

            // Courses to remove (toggled OFF)
            var coursesToRemove = existingCourseIds.Except(enrolledCourseIds).ToList();
            foreach (var courseId in coursesToRemove)
            {
                var enrollment = existingEnrollments.First(e => e.CourseId == courseId);
                enrollment.IsDeleted = true;
                enrollment.ModifiedById = modifiedBy;
                enrollment.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

    }
}
