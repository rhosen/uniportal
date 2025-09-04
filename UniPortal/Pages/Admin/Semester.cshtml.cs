using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Services;

namespace UniPortal.Pages.Admin
{
    public class SemesterModel : PageModel
    {
        private readonly SemesterService _semesterService;

        public SemesterModel(SemesterService semesterService)
        {
            _semesterService = semesterService;
        }

        public List<Data.Entities.Semester> Semesters { get; set; } = new();

        [BindProperty] public Data.Entities.Semester NewSemester { get; set; } = new();
        [BindProperty] public Data.Entities.Semester EditSemester { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string EditSemesterId { get; set; }

        // Search & Pagination
        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            var allSemesters = await _semesterService.GetAllAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allSemesters = allSemesters
                    .Where(s => s.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            TotalPages = (int)Math.Ceiling(allSemesters.Count / (double)PageSize);
            Semesters = allSemesters
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            await _semesterService.CreateAsync(NewSemester.Name, NewSemester.StartDate, NewSemester.EndDate);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditSemesterId = id;
            var semester = await _semesterService.GetByIdAsync(id);
            if (semester != null)
            {
                EditSemester = new Data.Entities.Semester
                {
                    Id = semester.Id,
                    Name = semester.Name,
                    StartDate = semester.StartDate,
                    EndDate = semester.EndDate
                };
            }
            await OnGetAsync();
            return Page();
        }

        public IActionResult OnPostCancelEdit()
        {
            EditSemesterId = null;
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostSaveEditAsync(string id)
        {
            await _semesterService.UpdateAsync(Guid.Parse(id), EditSemester.Name, EditSemester.StartDate, EditSemester.EndDate);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await _semesterService.DeleteAsync(id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            await _semesterService.ActivateAsync(id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
