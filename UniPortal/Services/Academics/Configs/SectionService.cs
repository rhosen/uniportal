using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;

namespace UniPortal.Services.Academics.Configs
{
    public class SectionService
    {
        private readonly UniPortalContext _context;

        public SectionService(UniPortalContext context)
        {
            _context = context;
        }

        // Get all sections
        public async Task<List<Section>> GetAllAsync()
        {
            return await _context.Sections
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        // Get section by Id
        public async Task<Section?> GetByIdAsync(Guid id)
        {
            return await _context.Sections
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }

        // Create new section
        public async Task CreateAsync(string name)
        {
            var section = new Section
            {
                Name = name
            };
            _context.Sections.Add(section);
            await _context.SaveChangesAsync();
        }

        // Update section
        public async Task UpdateAsync(Guid id, string name)
        {
            var section = await _context.Sections.FindAsync(id);
            if (section == null) return;

            section.Name = name;
            section.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        // Soft delete with validation
        public async Task DeleteAsync(Guid id)
        {
            var section = await _context.Sections
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (section == null) return;

            // Check if section is used in students or course offerings
            var isUsed = await _context.Students.AnyAsync(s => s.SectionId == id && !s.IsDeleted) ||
                         await _context.CourseOfferings.AnyAsync(co => co.SectionId == id && !co.IsDeleted);

            if (isUsed)
                throw new InvalidOperationException(
                    "This section cannot be deleted because it is assigned to students or course offerings."
                );

            section.IsDeleted = true;
            section.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        // Reactivate section
        public async Task ActivateAsync(Guid id)
        {
            var section = await _context.Sections.FindAsync(id);
            if (section == null) return;

            section.IsDeleted = false;
            section.DeletedAt = null;
            await _context.SaveChangesAsync();
        }

        // Dropdown options
        public async Task<List<SelectOption>> GetSectionOptionsAsync()
        {
            return await _context.Sections
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Name)
                .Select(s => new SelectOption
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .ToListAsync();
        }
    }
}
