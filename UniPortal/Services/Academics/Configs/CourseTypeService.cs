using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;
using UniPortal.Services.Infrastructures;

namespace UniPortal.Services.Academics.Configs
{
    public class CourseTypeService : BaseService<CourseType>
    {
        public CourseTypeService(UniPortalContext context, LogService logService)
            : base(context, logService)
        {
        }


        public async Task<List<SelectOption>> GetSelectOptionsAsync()
        {
            return await _context.CourseTypes
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.Name)
                .Select(r => new SelectOption
                {
                    Id = r.Id,
                    Name = r.Name
                })
                .ToListAsync();
        }

        public async Task<List<CourseType>> GetAllAsync()
        {
            return await _context.CourseTypes
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<CourseType?> GetByIdAsync(Guid id)
        {
            return await _context.CourseTypes
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        public async Task<CourseType> CreateAsync(string name, Guid createdById)
        {
            var type = new CourseType
            {
                Id = Guid.NewGuid(),
                Name = name,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            _context.CourseTypes.Add(type);
            await _context.SaveChangesAsync();

            await LogAsync(createdById, ActionType.Create, nameof(CourseType), type.Id, new { Name = name });

            return type;
        }

        public async Task UpdateAsync(Guid id, string name, Guid updatedById)
        {
            var type = await _context.CourseTypes.FindAsync(id);
            if (type == null) return;

            var oldValues = new { type.Name };

            type.Name = name;
            type.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            await LogAsync(
                updatedById,
                ActionType.Update,
                nameof(CourseType),
                type.Id,
                new { Old = oldValues, New = new { Name = name } }
            );
        }

        public async Task DeleteAsync(Guid id, Guid deletedById)
        {
            var type = await _context.CourseTypes.FindAsync(id);
            if (type == null) return;

            type.IsDeleted = true;
            type.DeletedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            await LogAsync(deletedById, ActionType.Delete, nameof(CourseType), type.Id);
        }

        public async Task ActivateAsync(Guid id, Guid activatedById)
        {
            var type = await _context.CourseTypes.FindAsync(id);
            if (type == null) return;

            type.IsDeleted = false;
            type.DeletedAt = null;

            await _context.SaveChangesAsync();
            await LogAsync(activatedById, ActionType.Activate, nameof(CourseType), type.Id);
        }
    }
}
