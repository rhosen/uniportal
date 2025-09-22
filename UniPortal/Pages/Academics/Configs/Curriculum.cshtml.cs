using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Dtos;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Academics.Configs
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class CurriculumModel : BasePageModel
    {
        private readonly CurriculumService _curriculumService;
        private readonly ProgramService _programService;
        private readonly SemesterService _semesterService;
        private readonly CourseService _courseService;
        private readonly CourseTypeService _courseTypeService;

        public CurriculumModel(
            CurriculumService curriculumService,
            ProgramService programService,
            SemesterService semesterService,
            CourseService courseService,
            CourseTypeService courseTypeService,
            AccountService accountService)
            : base(accountService)
        {
            _curriculumService = curriculumService;
            _programService = programService;
            _semesterService = semesterService;
            _courseService = courseService;
            _courseTypeService = courseTypeService;
        }

        // DTO list for page display
        public List<CurriculumDto> Curriculums { get; set; } = new();

        // SelectOption lists for dropdowns
        public List<SelectOption> ProgramOptions { get; set; } = new();
        public List<SelectOption> SemesterOptions { get; set; } = new();
        public List<SemesterOption> SemesterNumberOptions { get; set; } = new();
        public List<SelectOption> CourseOptions { get; set; } = new();
        public List<SelectOption> CourseTypeOptions { get; set; } = new();

        [BindProperty] public CurriculumDto NewCurriculum { get; set; } = new();
        [BindProperty] public CurriculumDto EditCurriculum { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string EditCurriculumId { get; set; }

        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            SemesterNumberOptions = _semesterService.GetSemesterNumberOptions();
            ProgramOptions = await _programService.GetSelectOptionsAsync();
            SemesterOptions = await _semesterService.GetSelectOptionsAsync();
            CourseOptions = await _courseService.GetSelectOptionsAsync();
            CourseTypeOptions = await _courseTypeService.GetSelectOptionsAsync();

            // Get all curriculums
            var allCurriculums = await _curriculumService.GetAllAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allCurriculums = allCurriculums
                    .Where(c =>
                        c.ProgramName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        c.CourseTitle.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            TotalPages = (int)Math.Ceiling(allCurriculums.Count / (double)PageSize);
            Curriculums = allCurriculums
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            await R(() => _curriculumService.CreateAsync(
                NewCurriculum.ProgramId,
                EditCurriculum.SemesterNumber,
                NewCurriculum.CourseId,
                NewCurriculum.SequenceOrder,
                CurrentAccount.Id
            ), "Curriculum created successfully");


            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditCurriculumId = id;
            var item = await _curriculumService.GetByIdAsync(Guid.Parse(id));
            if (item != null)
            {
                EditCurriculum = item;
            }
            await OnGetAsync();
            return Page();
        }

        public IActionResult OnPostCancelEdit()
        {
            EditCurriculumId = null;
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostSaveEditAsync(string id)
        {
           await R(() => _curriculumService.UpdateAsync(
                Guid.Parse(id),
                EditCurriculum.ProgramId,
                EditCurriculum.SemesterNumber,
                EditCurriculum.CourseId,
                EditCurriculum.SequenceOrder,
                CurrentAccount.Id
            ), "Curriculum updated successfully");


            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            await R(() => _curriculumService.DeleteAsync(id, CurrentAccount.Id), "Curriculum deleted successfully.");
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }


        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            await _curriculumService.ActivateAsync(Guid.Parse(id), CurrentAccount.Id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
