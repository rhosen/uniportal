using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;
using UniPortal.Dtos;
using UniPortal.Services.Infrastructures;

namespace UniPortal.Services.Academics.Configs
{
    public class SectionService : BaseService<Section>
    {
        public SectionService(IUnitOfWork unitOfWork, LogService logService)
            : base(unitOfWork.Context, logService)
        {
        }

        public async Task<List<Section>> GetAllAsync()
        {
            return await _context.Sections
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

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

        public async Task<Section> GetByIdAsync(Guid id)
        {
            return await _context.Sections
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }
    }
}
