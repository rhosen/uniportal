using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;
using UniPortal.Services.Infrastructures;

namespace UniPortal.Services.Academics.Configs
{
    public class FacultyTypeService : BaseService<FacultyType>
    {
        private readonly IUnitOfWork _unitOfWork;

        public FacultyTypeService(UniPortalContext context, LogService logService, IUnitOfWork unitOfWork)
            : base(context, logService)
        {
            _unitOfWork = unitOfWork;
        }

        #region Select Options

        public async Task<List<SelectOption>> GetSelectOptionsAsync()
        {
            return await _unitOfWork.Context.FacultyTypes
                .Where(ft => !ft.IsDeleted)
                .OrderBy(ft => ft.Name)
                .Select(ft => new SelectOption
                {
                    Id = ft.Id,
                    Name = ft.Name
                })
                .ToListAsync();
        }

        #endregion

        #region CRUD

        public async Task<FacultyType> CreateAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Faculty type name cannot be empty.");

            // Check uniqueness
            if (await _unitOfWork.Context.FacultyTypes.AnyAsync(ft => ft.Name == name && !ft.IsDeleted))
                throw new InvalidOperationException($"Faculty type '{name}' already exists.");

            var entity = new FacultyType
            {
                Name = name
            };

            _unitOfWork.Context.FacultyTypes.Add(entity);
            await _unitOfWork.CommitAsync();

            await LogAsync(null, ActionType.Create, nameof(FacultyType), entity.Id, entity);
            return entity;
        }

        public async Task<FacultyType> GetAsync(Guid id)
        {
            return await _unitOfWork.Context.FacultyTypes
                .FirstOrDefaultAsync(ft => ft.Id == id && !ft.IsDeleted);
        }

        public async Task<List<FacultyType>> GetAllAsync()
        {
            return await _unitOfWork.Context.FacultyTypes
                .Where(ft => !ft.IsDeleted)
                .OrderBy(ft => ft.Name)
                .ToListAsync();
        }

        public async Task UpdateAsync(Guid id, string name)
        {
            var entity = await _unitOfWork.Context.FacultyTypes.FirstOrDefaultAsync(ft => ft.Id == id);
            if (entity == null) throw new Exception("Faculty type not found.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Faculty type name cannot be empty.");

            if (await _unitOfWork.Context.FacultyTypes
                    .AnyAsync(ft => ft.Name == name && ft.Id != id && !ft.IsDeleted))
                throw new InvalidOperationException($"Faculty type '{name}' already exists.");

            entity.Name = name;
            _unitOfWork.Context.FacultyTypes.Update(entity);
            await _unitOfWork.CommitAsync();

            await LogAsync(null, ActionType.Update, nameof(FacultyType), entity.Id, entity);
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.Context.FacultyTypes.FirstOrDefaultAsync(ft => ft.Id == id);
            if (entity == null) return;

            // Check dependency: any faculty using this type?
            bool hasDependency = await _unitOfWork.Context.Faculties.AnyAsync(f => f.FacultyTypeId == id && !f.IsDeleted);
            if (hasDependency)
                throw new InvalidOperationException("Cannot delete faculty type because it is assigned to one or more faculties.");

            entity.IsDeleted = true;
            _unitOfWork.Context.FacultyTypes.Update(entity);
            await _unitOfWork.CommitAsync();

            await LogAsync(null, ActionType.Delete, nameof(FacultyType), entity.Id, entity);
        }

        public async Task ActivateAsync(Guid id)
        {
            var entity = await _unitOfWork.Context.FacultyTypes.FirstOrDefaultAsync(ft => ft.Id == id);
            if (entity == null) return;

            entity.IsDeleted = false;
            _unitOfWork.Context.FacultyTypes.Update(entity);
            await _unitOfWork.CommitAsync();

            await LogAsync(null, ActionType.Activate, nameof(FacultyType), entity.Id, entity);
        }

        #endregion
    }
}
