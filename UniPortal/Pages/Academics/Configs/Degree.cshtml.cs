using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Academics.Configs
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class DegreeModel : BasePageModel
    {
        private readonly DegreeService _degreeService;

        public DegreeModel(DegreeService degreeService, AccountService accountService) : base(accountService)
        {
            _degreeService = degreeService;
        }

        public List<Degree> Degrees { get; set; } = new();

        [BindProperty] public Degree NewDegree { get; set; } = new();
        [BindProperty] public Degree EditDegree { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string? EditDegreeId { get; set; }

        // Search & Pagination
        [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            var allDegrees = await _degreeService.GetAllAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allDegrees = allDegrees
                    .Where(d => d.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            TotalPages = (int)Math.Ceiling(allDegrees.Count / (double)PageSize);
            Degrees = allDegrees
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!string.IsNullOrWhiteSpace(NewDegree.Name))
            {
                await _degreeService.CreateAsync(NewDegree.Name);
            }
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditDegreeId = id;
            if (!Guid.TryParse(id, out var degreeId))
                return RedirectToPage();

            var degree = await _degreeService.GetByIdAsync(degreeId);
            if (degree != null)
            {
                EditDegree = new Degree
                {
                    Id = degree.Id,
                    Name = degree.Name
                };
            }

            await OnGetAsync();
            return Page();
        }

        public IActionResult OnPostCancelEdit()
        {
            EditDegreeId = null;
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostSaveEditAsync(string id)
        {
            if (!Guid.TryParse(id, out var degreeId))
                return RedirectToPage(new { CurrentPage, SearchTerm });

            await _degreeService.UpdateAsync(degreeId, EditDegree.Name);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (Guid.TryParse(id, out var degreeId))
            {
                await R(() => _degreeService.DeleteAsync(degreeId), "Degree deleted successfully.");
            }
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }


        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            if (Guid.TryParse(id, out var degreeId))
            {
                await _degreeService.ActivateAsync(degreeId);
            }
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
