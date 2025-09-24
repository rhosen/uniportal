using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Academics.Configs
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class InstitutionModel : BasePageModel
    {
        private readonly InstitutionService _institutionService;

        public InstitutionModel(InstitutionService institutionService,
                                AccountService accountService) : base(accountService)
        {
            _institutionService = institutionService;
        }

        public List<Institution> Institutions { get; set; } = new();

        [BindProperty] public string? EditInstitutionId { get; set; }
        [BindProperty] public Institution EditInstitution { get; set; } = new();
        [TempData] public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            Institutions = await _institutionService.GetAllAsync();
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditInstitutionId = id;
            if (!Guid.TryParse(id, out var instId))
                return RedirectToPage();

            var inst = await _institutionService.GetByIdAsync(instId);
            if (inst != null)
            {
                EditInstitution = new Institution
                {
                    Id = inst.Id,
                    Name = inst.Name,
                    Address = inst.Address,
                    Email = inst.Email,
                    Phone = inst.Phone,
                    LogoUrl = inst.LogoUrl
                };
            }

            await OnGetAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCancelEdit()
        {
            EditInstitutionId = null;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSaveEditAsync(string id)
        {
            if (!Guid.TryParse(id, out var instId))
                return RedirectToPage();

            try
            {
                await _institutionService.UpdateAsync(
                    instId,
                    EditInstitution.Name,
                    EditInstitution.Address,
                    EditInstitution.Email,
                    EditInstitution.Phone,
                    EditInstitution.LogoUrl
                );
                EditInstitutionId = null;
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
            }

            await OnGetAsync();
            return Page();
        }
    }
}
