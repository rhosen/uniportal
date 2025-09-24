using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;

namespace UniPortal.Services.Academics.Configs
{
    public class CourseTypeService
    {
        private readonly UniPortalContext _context;

        public CourseTypeService(UniPortalContext context)
        {
            _context = context;
        }

        // Get all CourseTypes
        public async Task<List<CourseType>> GetAllAsync()
        {
            return await _context.CourseTypes
                .Where(ct => !ct.IsDeleted)
                .OrderBy(ct => ct.Name)
                .ToListAsync();
        }

        // Get options for dropdowns
        public async Task<List<SelectOption>> GetSelectOptionsAsync()
        {
            return await _context.CourseTypes
                .Where(ct => !ct.IsDeleted)
                .OrderBy(ct => ct.Name)
                .Select(ct => new SelectOption
                {
                    Id = ct.Id,
                    Name = ct.Name
                })
                .ToListAsync();
        }

        // Get CourseType by Id
        public async Task<CourseType?> GetByIdAsync(Guid id)
        {
            return await _context.CourseTypes
                .FirstOrDefaultAsync(ct => ct.Id == id && !ct.IsDeleted);
        }

        // Create new CourseType
        public async Task CreateAsync(string name)
        {
            var courseType = new CourseType
            {
                Name = name
            };
            _context.CourseTypes.Add(courseType);
            await _context.SaveChangesAsync();
        }

        // Update existing CourseType
        public async Task UpdateAsync(Guid id, string name)
        {
            var courseType = await _context.CourseTypes.FindAsync(id);
            if (courseType == null) return;

            courseType.Name = name;
            courseType.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        // Soft delete CourseType
        public async Task DeleteAsync(Guid id)
        {
            var courseType = await _context.CourseTypes.FirstOrDefaultAsync(ct => ct.Id == id && !ct.IsDeleted);
            if (courseType == null) return;

            // Check if it is used in any course
            var isUsed = await _context.Courses.AnyAsync(c => c.CourseTypeId == id && !c.IsDeleted);
            if (isUsed)
                throw new InvalidOperationException("This course type is used in active courses and cannot be deleted.");

            courseType.IsDeleted = true;
            courseType.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        // Reactivate soft-deleted CourseType
        public async Task ActivateAsync(Guid id)
        {
            var courseType = await _context.CourseTypes.FindAsync(id);
            if (courseType == null) return;

            courseType.IsDeleted = false;
            courseType.DeletedAt = null;
            await _context.SaveChangesAsync();
        }
    }
}
