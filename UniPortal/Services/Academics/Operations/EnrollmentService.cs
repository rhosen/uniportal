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

        public async Task<List<SelectOption>> GetCoursesForTeacherAsync(Guid teacherId, Guid semesterId)
        {
            var courses = await _context.Courses
                .AsNoTracking()
                .Where(c => c.TeacherId == teacherId && c.SemesterId == semesterId)
                .Select(c => new
                {
                    c.Id,
                    c.Subject.Code,
                    c.Subject.Name
                })
                .OrderBy(c => c.Code)
                .ToListAsync();

            return courses
                .Select(c => new SelectOption
                {
                    Id = c.Id,
                    Name = $"{c.Code} - {c.Name}"
                })
                .ToList();
        }

        // Get enrollments for current semester
        public async Task<List<EnrollmentDto>> GetEnrollmentsForSemesterAsync(Guid semesterId)
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.Account)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Subject)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Department)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Teacher)
                .Where(e => !e.IsDeleted && e.Course.SemesterId == semesterId)
                .Select(e => new EnrollmentDto
                {
                    Id = e.Id.ToString(),
                    StudentId = e.Student.StudentId,
                    StudentName = e.Student.Account.FirstName + " " + e.Student.Account.LastName,
                    Department = e.Student.Department.Name,
                    CourseName = e.Course.Subject.Code + " - " + e.Course.Subject.Name,
                    TeacherName = e.Course.Teacher.FirstName + " " + e.Course.Teacher.LastName,
                    Credits = e.Course.Credits,
                    IsDeleted = e.IsDeleted
                })
                .ToListAsync();
        }

        // Enroll a student
        public async Task EnrollStudentAsync(Guid studentId, Guid courseId, Guid semesterId, Guid createdById)
        {
            try
            {
                var exists = await _context.Enrollments
               .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId && !e.IsDeleted);

                if (exists) return; // skip duplicate

                var enrollment = new Enrollment
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    ModifiedById = createdById,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        // Soft delete
        public async Task DeleteEnrollmentAsync(string id, Guid deletedById)
        {
            var enrollment = await _context.Enrollments.FindAsync(Guid.Parse(id));
            if (enrollment != null && !enrollment.IsDeleted)
            {
                enrollment.IsDeleted = true;
                enrollment.DeletedAt = DateTime.UtcNow;
                enrollment.ModifiedById = deletedById; // keep audit
                await _context.SaveChangesAsync();
            }
        }

        // Activate / undo delete
        public async Task ActivateEnrollmentAsync(string id, Guid createdById)
        {
            var enrollment = await _context.Enrollments.FindAsync(Guid.Parse(id));
            if (enrollment != null && enrollment.IsDeleted)
            {
                enrollment.IsDeleted = false;
                enrollment.DeletedAt = null;
                enrollment.ModifiedById = createdById; // audit
                await _context.SaveChangesAsync();
            }
        }
    }
}
