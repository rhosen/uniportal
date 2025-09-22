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

        // -----------------------------
        // Get select options
        // -----------------------------
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

        // -----------------------------
        // Get all courses
        // -----------------------------
        public async Task<List<Course>> GetAllAsync()
        {
            return await _context.Courses
                .Include(c => c.Department)
                .Include(c => c.CourseType)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Title)
                .ToListAsync();
        }

        // -----------------------------
        // Get course by Id
        // -----------------------------
        public async Task<Course?> GetByIdAsync(Guid courseId)
        {
            return await _context.Courses
                .Include(c => c.Department)
                .Include(c => c.CourseType)
                .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted);
        }

        // -----------------------------
        // Create course
        // -----------------------------
        public async Task<Course> CreateAsync(string code, string title, int creditHours, Guid departmentId, Guid courseTypeId, Guid userId)
        {
            var course = new Course
            {
                Id = Guid.NewGuid(),
                Code = code,
                Title = title,
                CreditHours = creditHours,
                DepartmentId = departmentId,
                CourseTypeId = courseTypeId,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            await LogAsync(
                userId,
                ActionType.Create,
                "Course",
                course.Id,
                new { Code = code, Title = title, CreditHours = creditHours, DepartmentId = departmentId, CourseTypeId = courseTypeId }
            );

            return course;
        }

        // -----------------------------
        // Update course
        // -----------------------------
        public async Task UpdateAsync(Guid courseId, string code, string title, int creditHours, Guid departmentId, Guid courseTypeId, Guid userId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return;

            var oldValues = new
            {
                course.Code,
                course.Title,
                course.CreditHours,
                course.DepartmentId,
                course.CourseTypeId
            };

            course.Code = code;
            course.Title = title;
            course.CreditHours = creditHours;
            course.DepartmentId = departmentId;
            course.CourseTypeId = courseTypeId;
            course.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            await LogAsync(
                userId,
                ActionType.Update,
                "Course",
                course.Id,
                new { Old = oldValues, New = new { Code = code, Title = title, CreditHours = creditHours, DepartmentId = departmentId, CourseTypeId = courseTypeId } }
            );
        }

        // -----------------------------
        // Soft delete course
        // -----------------------------
        public async Task DeleteAsync(Guid courseId, Guid userId)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null) return;

            // Check dependencies
            var hasCurriculums = await _context.Curriculums
                .AnyAsync(c => c.CourseId == courseId && !c.IsDeleted);

            var hasCourseOfferings = await _context.CourseOfferings
                .AnyAsync(co => co.CourseId == courseId && !co.IsDeleted);

            if (hasCurriculums || hasCourseOfferings)
                throw new InvalidOperationException("This course cannot be deleted because it is part of active curriculums or course offerings.");

            course.IsDeleted = true;
            course.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            await LogAsync(userId, ActionType.Delete, "Course", course.Id);
        }

        // -----------------------------
        // Activate course
        // -----------------------------
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
