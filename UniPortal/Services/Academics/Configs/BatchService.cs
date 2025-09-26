using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;

namespace UniPortal.Services.Academics.Configs
{
    public class BatchService
    {
        private readonly UniPortalContext _context;

        public BatchService(UniPortalContext context)
        {
            _context = context;
        }

        // Get all batches
        public async Task<List<Batch>> GetAllAsync()
        {
            return await _context.Batches
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.Name)
                .ToListAsync();
        }

        // Get batch by Id
        public async Task<Batch?> GetByIdAsync(Guid id)
        {
            return await _context.Batches
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
        }

        // Create a new batch
        public async Task CreateAsync(string number)
        {
            var batch = new Batch
            {
                Name = number
            };
            _context.Batches.Add(batch);
            await _context.SaveChangesAsync();
        }

        // Update batch
        public async Task UpdateAsync(Guid id, string number)
        {
            var batch = await _context.Batches.FindAsync(id);
            if (batch == null) return;

            batch.Name = number;
            batch.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        // Soft delete batch with validation
        public async Task DeleteAsync(Guid id)
        {
            var batch = await _context.Batches
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

            if (batch == null) return;

            // Check if batch is used in students or course offerings
            var isUsed = await _context.Students.AnyAsync(s => s.BatchId == id && !s.IsDeleted) ||
                         await _context.CourseOfferings.AnyAsync(co => co.BatchId == id && !co.IsDeleted);

            if (isUsed)
                throw new InvalidOperationException(
                    "This batch cannot be deleted because it is assigned to students or course offerings."
                );

            batch.IsDeleted = true;
            batch.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        // Reactivate batch
        public async Task ActivateAsync(Guid id)
        {
            var batch = await _context.Batches.FindAsync(id);
            if (batch == null) return;

            batch.IsDeleted = false;
            batch.DeletedAt = null;
            await _context.SaveChangesAsync();
        }

        // Dropdown options
        public async Task<List<SelectOption>> GetBatchOptionsAsync()
        {
            return await _context.Batches
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.Name)
                .Select(b => new SelectOption
                {
                    Id = b.Id,
                    Name = b.Name
                })
                .ToListAsync();
        }
    }
}
