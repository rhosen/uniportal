using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;

namespace UniPortal.Services.Faculty
{
    public class CourseService
    {
        private readonly UniPortalContext _context;

        public CourseService(UniPortalContext context)
        {
            _context = context;
        }

        public async Task<List<Course>> GetAllAsync()
        {
            return await _context.Courses
                .Include(c => c.Department)
                .Include(c => c.Teacher)
                .Include(c => c.Semester)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Course> GetByIdAsync(string id)
        {
            return await _context.Courses
                .Include(c => c.Department)
                .Include(c => c.Teacher)
                .Include(c => c.Semester)
                .FirstOrDefaultAsync(c => c.Id.ToString() == id);
        }

        public async Task CreateAsync(string name, string code, Guid departmentId, Guid teacherId, Guid semesterId, int credits = 3)
        {
            var course = new Course
            {
                Name = name,
                Code = code,
                DepartmentId = departmentId,
                TeacherId = teacherId,
                SemesterId = semesterId,
                Credits = credits
            };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, string name, string code, Guid departmentId, Guid teacherId, Guid semesterId, int credits = 3)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                course.Name = name;
                course.Code = code;
                course.DepartmentId = departmentId;
                course.TeacherId = teacherId;
                course.SemesterId = semesterId;
                course.Credits = credits;
                course.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(string id)
        {
            var course = await _context.Courses.FindAsync(Guid.Parse(id));
            if (course != null)
            {
                course.IsDeleted = true;
                course.DeletedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ActivateAsync(string id)
        {
            var course = await _context.Courses.FindAsync(Guid.Parse(id));
            if (course != null)
            {
                course.IsDeleted = false;
                course.DeletedAt = null;
                await _context.SaveChangesAsync();
            }
        }
    }
}
