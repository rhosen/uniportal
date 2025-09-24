using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Academics.Configs
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class FacultyTypeModel : BasePageModel
    {
        private readonly FacultyTypeService _facultyTypeService;

        public FacultyTypeModel(FacultyTypeService facultyTypeService,
                                AccountService accountService) : base(accountService)
        {
            _facultyTypeService = facultyTypeService;
        }

        public List<FacultyType> FacultyTypes { get; set; } = new();

        [BindProperty] public FacultyType NewFacultyType { get; set; } = new();
        [BindProperty] public FacultyType EditFacultyType { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string? EditFacultyTypeId { get; set; }

        // Search & Pagination
        [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            var allTypes = await _facultyTypeService.GetAllAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allTypes = allTypes
                    .Where(t => t.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            TotalPages = (int)Math.Ceiling(allTypes.Count / (double)PageSize);
            FacultyTypes = allTypes
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!string.IsNullOrWhiteSpace(NewFacultyType.Name))
            {
                await _facultyTypeService.CreateAsync(NewFacultyType.Name);
            }
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditFacultyTypeId = id;
            if (!Guid.TryParse(id, out var typeId))
                return RedirectToPage();

            var type = await _facultyTypeService.GetAsync(typeId);
            if (type != null)
            {
                EditFacultyType = type;
            }

            await OnGetAsync();
            return Page();
        }

        public IActionResult OnPostCancelEdit()
        {
            EditFacultyTypeId = null;
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostSaveEditAsync(string id)
        {
            if (!Guid.TryParse(id, out var typeId))
                return RedirectToPage(new { CurrentPage, SearchTerm });

            await _facultyTypeService.UpdateAsync(typeId, EditFacultyType.Name);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (Guid.TryParse(id, out var typeId))
            {
                await R(() => _facultyTypeService.DeleteAsync(typeId), "Faculty type deleted successfully.");
            }
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            if (Guid.TryParse(id, out var typeId))
            {
                await _facultyTypeService.ActivateAsync(typeId);
            }
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
