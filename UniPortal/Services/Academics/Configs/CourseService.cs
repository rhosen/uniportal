using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;
using UniPortal.Services.Infrastructures;

namespace UniPortal.Services.Academics.Configs
{
    public class CourseService : BaseService<Course>
    {
        public CourseService(UniPortalContext context, LogService logService)
            : base(context, logService)
        {
        }


        public async Task<List<SelectOption>> GetSelectOptionsAsync()
        {
            return await _context.Courses
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Title)
                .Select(c => new SelectOption
                {
                    Id = c.Id,
                    Name = c.Title
                })
                .ToListAsync();
        }


        // Get all courses
        public async Task<List<Course>> GetAllAsync()
        {
            return await _context.Courses
                .Include(c => c.Department)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Title)
                .ToListAsync();
        }

        // Get course by Id
        public async Task<Course?> GetByIdAsync(Guid courseId)
        {
            return await _context.Courses
                .Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted);
        }

        // Create course
        public async Task<Course> CreateAsync(string code, string title, int creditHours, Guid departmentId, Guid userId)
        {
            var course = new Course
            {
                Id = Guid.NewGuid(),
                Code = code,
                Title = title,
                CreditHours = creditHours,
                DepartmentId = departmentId
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            await LogAsync(
                userId,
                ActionType.Create,
                "Course",
                course.Id,
                new { Code = code, Title = title, CreditHours = creditHours, DepartmentId = departmentId }
            );

            return course;
        }

        // Update course
        public async Task UpdateAsync(Guid courseId, string code, string title, int creditHours, Guid departmentId, Guid userId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return;

            var oldValues = new { course.Code, course.Title, course.CreditHours, course.DepartmentId };

            course.Code = code;
            course.Title = title;
            course.CreditHours = creditHours;
            course.DepartmentId = departmentId;

            await _context.SaveChangesAsync();

            await LogAsync(
                userId,
                ActionType.Update,
                "Course",
                course.Id,
                new { Old = oldValues, New = new { Code = code, Title = title, CreditHours = creditHours, DepartmentId = departmentId } }
            );
        }

        // Soft delete course
        public async Task DeleteAsync(Guid courseId, Guid userId)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null) return;

            // Check if course is used in any active curriculums
            var hasCurriculums = await _context.Curriculums
                .AnyAsync(c => c.CourseId == courseId && !c.IsDeleted);

            // Check if course has any active course offerings
            var hasCourseOfferings = await _context.CourseOfferings
                .AnyAsync(co => co.CourseId == courseId && !co.IsDeleted);

            if (hasCurriculums || hasCourseOfferings)
            {
                throw new InvalidOperationException(
                    "This course cannot be deleted because it is part of active curriculums or course offerings."
                );
            }

            // Safe to soft delete
            course.IsDeleted = true;
            course.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            await LogAsync(userId, ActionType.Delete, "Course", course.Id);
        }

        // Activate (undo soft delete)
        public async Task ActivateAsync(Guid courseId, Guid userId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return;

            course.IsDeleted = false;
            await _context.SaveChangesAsync();

            await LogAsync(userId, ActionType.Activate, "Course", course.Id);
        }
    }
}
