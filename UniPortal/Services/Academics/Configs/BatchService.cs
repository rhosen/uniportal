using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;
using UniPortal.Dtos;
using UniPortal.Services.Infrastructures;

namespace UniPortal.Services.Academics.Configs
{
    public class BatchService : BaseService<Batch>
    {
        public BatchService(IUnitOfWork unitOfWork, LogService logService)
            : base(unitOfWork.Context, logService)
        {
        }

        public async Task<List<Batch>> GetAllAsync()
        {
            return await _context.Batches
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.Number)
                .ToListAsync();
        }

        public async Task<List<SelectOption>> GetBatchOptionsAsync()
        {
            return await _context.Batches
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.Number)
                .Select(b => new SelectOption
                {
                    Id = b.Id,
                    Name = b.Number
                })
                .ToListAsync();
        }

        public async Task<Batch> GetByIdAsync(Guid id)
        {
            return await _context.Batches
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
        }
    }
}
