using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities;
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
                .Include(c => c.Department)
                .Include(c => c.Teacher)
                .Include(c => c.Semester)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Subject.Name)
                .ToListAsync();
        }

        public async Task<List<Course>> GetOngoingCoursesAsync()
        {
            var currentDate = DateTime.Now;

            return await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Department)
                .Include(c => c.Teacher)
                .Include(c => c.Semester)
                .Where(c => !c.IsDeleted && c.Semester.EndDate > currentDate)
                .OrderBy(c => c.Subject.Name)
                .ToListAsync();
        }

        public async Task<Course> GetByIdAsync(Guid courseId)
        {
            return await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Department)
                .Include(c => c.Teacher)
                .Include(c => c.Semester)
                .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted);
        }

        public async Task<Course> CreateAsync(Guid subjectId, Guid departmentId, Guid teacherId, Guid semesterId, int credits = 3, Guid? createdById = null)
        {
            var course = new Course
            {
                SubjectId = subjectId,
                DepartmentId = departmentId,
                TeacherId = teacherId,
                SemesterId = semesterId,
                Credits = credits,
                CreatedAt = DateTime.Now
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            await LogAsync(createdById, ActionType.Create, AppConstant.Academic.Course, course.Id,
                new { SubjectId = subjectId, DepartmentId = departmentId, TeacherId = teacherId, SemesterId = semesterId, Credits = credits });

            return course;
        }

        public async Task UpdateAsync(Guid courseId, Guid subjectId, Guid departmentId, Guid teacherId, Guid semesterId, int credits = 3, Guid? updatedById = null)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return;

            var oldValues = new { course.SubjectId, course.DepartmentId, course.TeacherId, course.SemesterId, course.Credits };

            course.SubjectId = subjectId;
            course.DepartmentId = departmentId;
            course.TeacherId = teacherId;
            course.SemesterId = semesterId;
            course.Credits = credits;
            course.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            await LogAsync(updatedById, ActionType.Update, AppConstant.Academic.Course, course.Id,
                new { Old = oldValues, New = new { SubjectId = subjectId, DepartmentId = departmentId, TeacherId = teacherId, SemesterId = semesterId, Credits = credits } });
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
