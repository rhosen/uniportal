using Microsoft.EntityFrameworkCore;
using UniPortal.Data;

namespace UniPortal.Services.Academics.Configs
{
    public class ProgramService
    {
        private readonly UniPortalContext _context;

        public ProgramService(UniPortalContext context)
        {
            _context = context;
        }

        // Get all programs (include Department and Head)
        public async Task<List<Data.Entities.Program>> GetAllAsync()
        {
            return await _context.Programs
                .Include(p => p.Department)
                .Where(x=> !x.IsDeleted)
                .OrderBy(p => p.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        // Get program by Id
        public async Task<Data.Entities.Program?> GetByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out var guid)) return null;

            return await _context.Programs
                .Include(p => p.Department)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == guid);
        }

        // Create program
        public async Task CreateAsync(string code, string name, Guid departmentId)
        {
            var program = new Data.Entities.Program
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = name,
                DepartmentId = departmentId,
                CreatedAt = DateTime.Now
            };

            _context.Programs.Add(program);
            await _context.SaveChangesAsync();
        }

        // Update program
        public async Task UpdateAsync(Guid id, string code, string name, Guid departmentId)
        {
            var program = await _context.Programs.FirstOrDefaultAsync(p => p.Id == id);
            if (program != null)
            {
                program.Code = code;
                program.Name = name;
                program.DepartmentId = departmentId;
                program.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
            }
        }

        // Soft delete program
        public async Task DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out var guid)) return;

            var program = await _context.Programs.FirstOrDefaultAsync(p => p.Id == guid);
            if (program != null)
            {
                program.IsDeleted = true;
                program.DeletedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        // Activate (undo soft delete)
        public async Task ActivateAsync(string id)
        {
            if (!Guid.TryParse(id, out var guid)) return;

            var program = await _context.Programs.FirstOrDefaultAsync(p => p.Id == guid);
            if (program != null && program.IsDeleted)
            {
                program.IsDeleted = false;
                program.DeletedAt = null;
                await _context.SaveChangesAsync();
            }
        }
    }
}
