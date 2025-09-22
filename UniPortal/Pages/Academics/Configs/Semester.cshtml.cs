using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Academics.Configs
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class SemesterModel : BasePageModel
    {
        private readonly SemesterService _semesterService;
        private readonly IConfiguration _configuration;

        public SemesterModel(SemesterService semesterService,
            IConfiguration configuration,
            AccountService accountService) : base(accountService)
        {
            _semesterService = semesterService;
            _configuration = configuration;
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
            // Read default semester duration from appsettings
            int defaultDurationMonths = _configuration.GetValue("SemesterDurationMonths", 6);

            // Get the next semester window from the service
            var (startDate, endDate) = await _semesterService.GetNextSemesterWindowAsync(defaultDurationMonths);
            NewSemester.StartDate = startDate;
            NewSemester.EndDate = endDate;

            // Fetch all semesters once (for listing/filtering)
            var allSemesters = await _semesterService.GetAllAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allSemesters = allSemesters
                    .Where(s => $"{s.SemesterType} {s.AcademicYear}"
                        .Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
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
            var result = await _semesterService.CreateAsync(
                NewSemester.SemesterType,
                NewSemester.AcademicYear,
                NewSemester.StartDate,
                NewSemester.EndDate,
                NewSemester.IsCurrent
            );

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                await OnGetAsync();
                return Page();
            }

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
                    SemesterType = semester.SemesterType,
                    AcademicYear = semester.AcademicYear,
                    StartDate = semester.StartDate,
                    EndDate = semester.EndDate,
                    IsCurrent = semester.IsCurrent
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
            var result = await _semesterService.UpdateAsync(
                Guid.Parse(id),
                EditSemester.SemesterType,
                EditSemester.AcademicYear,
                EditSemester.StartDate,
                EditSemester.EndDate,
                EditSemester.IsCurrent
            );

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                await OnGetAsync();
                return Page();
            }

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }


        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await R(() => _semesterService.DeleteAsync(id), "Semester deleted successfully.");
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            await _semesterService.ActivateAsync(id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
