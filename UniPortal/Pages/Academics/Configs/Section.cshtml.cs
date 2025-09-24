using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Academics.Configs
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class SectionModel : BasePageModel
    {
        private readonly SectionService _sectionService;

        public SectionModel(SectionService sectionService, AccountService accountService) : base(accountService)
        {
            _sectionService = sectionService;
        }

        public List<Section> Sections { get; set; } = new();

        [BindProperty] public Section NewSection { get; set; } = new();
        [BindProperty] public Section EditSection { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string? EditSectionId { get; set; }

        // Search & Pagination
        [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            var allSections = await _sectionService.GetAllAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allSections = allSections
                    .Where(s => s.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            TotalPages = (int)Math.Ceiling(allSections.Count / (double)PageSize);
            Sections = allSections
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!string.IsNullOrWhiteSpace(NewSection.Name))
                await _sectionService.CreateAsync(NewSection.Name);

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditSectionId = id;
            if (!Guid.TryParse(id, out var sectionId))
                return RedirectToPage();

            var section = await _sectionService.GetByIdAsync(sectionId);
            if (section != null)
            {
                EditSection = new Section
                {
                    Id = section.Id,
                    Name = section.Name
                };
            }

            await OnGetAsync();
            return Page();
        }

        public IActionResult OnPostCancelEdit()
        {
            EditSectionId = null;
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostSaveEditAsync(string id)
        {
            if (!Guid.TryParse(id, out var sectionId))
                return RedirectToPage(new { CurrentPage, SearchTerm });

            await _sectionService.UpdateAsync(sectionId, EditSection.Name);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (Guid.TryParse(id, out var sectionId))
            {
                await R(() => _sectionService.DeleteAsync(sectionId), "Section deleted successfully.");
            }

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            if (Guid.TryParse(id, out var sectionId))
            {
                await _sectionService.ActivateAsync(sectionId);
            }

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
