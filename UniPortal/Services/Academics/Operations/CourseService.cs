using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;
using UniPortal.Services.Infrastructures;

namespace UniPortal.Services.Academics.Operations
{
    public class CourseService : BaseService<Course>
    {
        public CourseService(UniPortalContext context, LogService logService)
            : base(context, logService)
        {
        }

        public async Task<List<Course>> GetAllAsync()
        {
            return await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Teacher)
                .Include(c => c.Semester)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Subject.Name)
                .ToListAsync();
        }

        public async Task<List<CourseDto>> GetCoursesForSemesterAsync(Guid semesterId)
        {
            var query = from c in _context.Courses
                        join s in _context.Subjects on c.SubjectId equals s.Id
                        join t in _context.Accounts on c.TeacherId equals t.Id
                        where c.SemesterId == semesterId
                        select new CourseDto
                        {
                            Id = c.Id,
                            CourseName = s.Code + " - " + s.Name,
                            TeacherName = t.FirstName + " " + t.LastName,
                            Credits = c.Credits
                        };

            return await query.ToListAsync();
        }


        public async Task<Course> GetByIdAsync(Guid courseId)
        {
            return await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Teacher)
                .Include(c => c.Semester)
                .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted);
        }

        public async Task<Course> CreateAsync(Guid subjectId, Guid teacherId, Guid semesterId, int credits = 3, Guid? createdById = null)
        {
            var course = new Course
            {
                SubjectId = subjectId,
                TeacherId = teacherId,
                SemesterId = semesterId,
                Credits = credits,
                CreatedAt = DateTime.Now
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            await LogAsync(createdById, ActionType.Create, AppConstant.Academic.Course, course.Id,
                new { SubjectId = subjectId, TeacherId = teacherId, SemesterId = semesterId, Credits = credits });

            return course;
        }

        public async Task UpdateAsync(Guid courseId, Guid subjectId, Guid teacherId, Guid semesterId, int credits = 3, Guid? updatedById = null)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return;

            var oldValues = new { course.SubjectId, course.TeacherId, course.SemesterId, course.Credits };

            course.SubjectId = subjectId;
            course.TeacherId = teacherId;
            course.SemesterId = semesterId;
            course.Credits = credits;
            course.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            await LogAsync(updatedById, ActionType.Update, AppConstant.Academic.Course, course.Id,
                new { Old = oldValues, New = new { SubjectId = subjectId, TeacherId = teacherId, SemesterId = semesterId, Credits = credits } });
        }

        public async Task DeleteAsync(Guid courseId, Guid? deletedById = null)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return;

            course.IsDeleted = true;
            course.DeletedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            await LogAsync(deletedById, ActionType.Delete, AppConstant.Academic.Course, course.Id);
        }

        public async Task ActivateAsync(Guid courseId, Guid? activatedById = null)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return;

            course.IsDeleted = false;
            course.DeletedAt = null;

            await _context.SaveChangesAsync();
            await LogAsync(activatedById, ActionType.Activate, AppConstant.Academic.Course, course.Id);
        }
    }
}
