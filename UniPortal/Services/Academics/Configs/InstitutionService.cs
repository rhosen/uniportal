using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Services.Infrastructures;

namespace UniPortal.Services.Academics.Configs
{
    public class InstitutionService : BaseService<Institution>
    {
        private readonly IUnitOfWork _unitOfWork;

        public InstitutionService(UniPortalContext context, LogService logService, IUnitOfWork unitOfWork)
            : base(context, logService)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Institution>> GetAllAsync()
        {
            return await _unitOfWork.Context.Institutions
                .Where(i => !i.IsDeleted)
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public async Task<Institution?> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.Context.Institutions
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        }

        public async Task CreateAsync(string name, string? address = null, string? email = null,
                                      string? phone = null, string? logoUrl = null)
        {
            var institution = new Institution
            {
                Name = name,
                Address = address,
                Email = email,
                Phone = phone,
                LogoUrl = logoUrl
            };

            _unitOfWork.Context.Institutions.Add(institution);
            await _unitOfWork.CommitAsync();

            await LogAsync(null, ActionType.Create, nameof(Institution), institution.Id, institution);
        }

        public async Task UpdateAsync(Guid id, string name, string? address = null, string? email = null,
                                      string? phone = null, string? logoUrl = null)
        {
            var institution = await GetByIdAsync(id);
            if (institution == null)
                throw new Exception("Institutions not found");

            institution.Name = name;
            institution.Address = address;
            institution.Email = email;
            institution.Phone = phone;
            institution.LogoUrl = logoUrl;

            _unitOfWork.Context.Institutions.Update(institution);
            await _unitOfWork.CommitAsync();

            await LogAsync(null, ActionType.Update, nameof(Institution), institution.Id, institution);
        }

        public async Task DeleteAsync(Guid id)
        {
            var institution = await GetByIdAsync(id);
            if (institution == null) return;

            institution.IsDeleted = true;
            institution.DeletedAt = DateTime.Now;

            _unitOfWork.Context.Institutions.Update(institution);
            await _unitOfWork.CommitAsync();

            await LogAsync(null, ActionType.Delete, nameof(Institution), institution.Id, institution);
        }

        public async Task ActivateAsync(Guid id)
        {
            var institution = await _unitOfWork.Context.Institutions.FirstOrDefaultAsync(i => i.Id == id);
            if (institution == null) return;

            institution.IsDeleted = false;
            institution.DeletedAt = null;

            _unitOfWork.Context.Institutions.Update(institution);
            await _unitOfWork.CommitAsync();

            await LogAsync(null, ActionType.Activate, nameof(Institution), institution.Id, institution);
        }
    }
}
