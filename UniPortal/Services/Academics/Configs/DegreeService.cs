using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;

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
                .AsNoTracking()
                .ToListAsync();
        }

        // Get degree by Id
        public async Task<Degree?> GetByIdAsync(Guid id)
        {
            return await _context.Degrees
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        }

        // Create new degree
        public async Task CreateAsync(string name)
        {
            var degree = new Degree
            {
                Id = Guid.NewGuid(),
                Name = name,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            _context.Degrees.Add(degree);
            await _context.SaveChangesAsync();
        }

        // Update existing degree
        public async Task UpdateAsync(Guid id, string name)
        {
            var degree = await _context.Degrees.FirstOrDefaultAsync(d => d.Id == id);
            if (degree != null)
            {
                degree.Name = name;
                degree.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        // Soft delete degree
        public async Task DeleteAsync(Guid id)
        {
            var degree = await _context.Degrees.FirstOrDefaultAsync(d => d.Id == id);
            if (degree != null)
            {
                degree.IsDeleted = true;
                degree.DeletedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        // Activate (undo soft delete)
        public async Task ActivateAsync(Guid id)
        {
            var degree = await _context.Degrees.FirstOrDefaultAsync(d => d.Id == id);
            if (degree != null && degree.IsDeleted)
            {
                degree.IsDeleted = false;
                degree.DeletedAt = null;
                await _context.SaveChangesAsync();
            }
        }
    }
}
