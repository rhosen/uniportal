using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;

namespace UniPortal.Services.Academics.Configs
{
    public class RecipientService
    {
        private readonly UniPortalContext _context;

        public RecipientService(UniPortalContext context)
        {
            _context = context;
        }

        // Get all recipients
        public async Task<List<Recipient>> GetAllAsync()
        {
            return await _context.Recipients
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        // Get recipient by Id
        public async Task<Recipient?> GetByIdAsync(Guid id)
        {
            return await _context.Recipients
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        // Create new recipient
        public async Task CreateAsync(string name, string? description = null)
        {
            var rec = new Recipient
            {
                Name = name,
                Description = description
            };
            _context.Recipients.Add(rec);
            await _context.SaveChangesAsync();
        }

        // Update recipient
        public async Task UpdateAsync(Guid id, string name, string? description = null)
        {
            var rec = await _context.Recipients.FindAsync(id);
            if (rec == null) return;

            rec.Name = name;
            rec.Description = description;
            rec.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        // Soft delete
        public async Task DeleteAsync(Guid id)
        {
            var rec = await _context.Recipients
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
            if (rec == null) return;

            rec.IsDeleted = true;
            rec.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        // Reactivate
        public async Task ActivateAsync(Guid id)
        {
            var rec = await _context.Recipients.FindAsync(id);
            if (rec == null) return;

            rec.IsDeleted = false;
            rec.DeletedAt = null;
            await _context.SaveChangesAsync();
        }

        // For dropdowns/selects
        public async Task<List<SelectOption>> GetOptionsAsync()
        {
            return await _context.Recipients
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.Name)
                .Select(r => new SelectOption
                {
                    Id = r.Id,
                    Name = r.Name
                })
                .ToListAsync();
        }
    }
}
