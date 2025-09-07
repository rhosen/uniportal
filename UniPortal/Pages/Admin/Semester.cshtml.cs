using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Constants;
using UniPortal.Services.Faculty;

namespace UniPortal.Pages.Admin
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
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
            // Set default start date to today
            NewSemester.StartDate = DateTime.Today;
            // Set default end date to 6 months from today
            NewSemester.EndDate = DateTime.Today.AddMonths(6);

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
