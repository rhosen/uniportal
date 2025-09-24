using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;

namespace UniPortal.Services.Academics.Configs
{
    public class DegreeService
    {
        private readonly UniPortalContext _context;

        public DegreeService(UniPortalContext context)
        {
            _context = context;
        }

        // Get all degrees
        public async Task<List<Degree>> GetAllAsync()
        {
            return await _context.Degrees
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        // Get degree by Id
        public async Task<Degree?> GetByIdAsync(Guid id)
        {
            return await _context.Degrees
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        }

        // Create new degree
        public async Task CreateAsync(string name)
        {
            var degree = new Degree
            {
                Name = name
            };
            _context.Degrees.Add(degree);
            await _context.SaveChangesAsync();
        }

        // Update degree
        public async Task UpdateAsync(Guid id, string name)
        {
            var degree = await _context.Degrees.FindAsync(id);
            if (degree == null) return;

            degree.Name = name;
            degree.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        // Soft delete with validation
        public async Task DeleteAsync(Guid id)
        {
            var degree = await _context.Degrees
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

            if (degree == null) return;

            // Check if degree is used in programs
            var isUsed = await _context.Programs.AnyAsync(p => p.DegreeId == id && !p.IsDeleted);

            if (isUsed)
                throw new InvalidOperationException(
                    "This degree cannot be deleted because it is assigned to existing programs."
                );

            degree.IsDeleted = true;
            degree.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        // Reactivate degree
        public async Task ActivateAsync(Guid id)
        {
            var degree = await _context.Degrees.FindAsync(id);
            if (degree == null) return;

            degree.IsDeleted = false;
            degree.DeletedAt = null;
            await _context.SaveChangesAsync();
        }

        // Dropdown options
        public async Task<List<SelectOption>> GetDegreeOptionsAsync()
        {
            return await _context.Degrees
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .Select(d => new SelectOption
                {
                    Id = d.Id,
                    Name = d.Name
                })
                .ToListAsync();
        }
    }
}
