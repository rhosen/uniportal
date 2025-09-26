using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Dtos;
using UniPortal.Dtos.Classwork;
using UniPortal.Helpers;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Academics.Operations;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Academics.Operations
{
    [Authorize(Roles = Roles.Faculty)]
    public class ClassworkModel : BasePageModel
    {
        private readonly ClassworkService _classworkService;
        private readonly FacultyService _facultyService;
        private readonly SemesterService _semesterService;
        private readonly BatchService _batchService;
        private readonly SectionService _sectionService;

        public ClassworkModel(
            ClassworkService classworkService,
            AccountService accountService,
            FacultyService facultyService,
            SemesterService semesterService,
            BatchService batchService,
            SectionService sectionService
        ) : base(accountService)
        {
            _classworkService = classworkService;
            _facultyService = facultyService;
            _semesterService = semesterService;
            _batchService = batchService;
            _sectionService = sectionService;
        }

        // Header dropdowns
        public List<SelectOption> SemesterOptions { get; set; } = new();
        public List<SelectOption> BatchOptions { get; set; } = new();
        public List<SelectOption> SectionOptions { get; set; } = new();
        [BindProperty(SupportsGet = true)] public Guid? SelectedSemesterId { get; set; }
        [BindProperty(SupportsGet = true)] public Guid? SelectedBatchId { get; set; }
        [BindProperty(SupportsGet = true)] public Guid? SelectedSectionId { get; set; }
        public List<ClassworkDto> Courses { get; set; } = new();
        [BindProperty(SupportsGet = true)] public Guid? SelectedCourseId { get; set; }
        public ClassworkDto SelectedCourse { get; set; }

        // Middle panel: classworks
        public List<ClassworkDto> Classworks { get; set; } = new();
        [BindProperty(SupportsGet = true)] public Guid? SelectedClassworkId { get; set; }
        public ClassworkDto SelectedClasswork { get; set; }
        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }

        public int PageSize { get; set; } = 10;
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int TotalClassworks { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalClassworks / (double)PageSize);

        // Right panel: submissions / form
        [BindProperty] public ClassworkCreateUpdateDto ClassworkDto { get; set; } = new();
        public List<ClassworkSubmissionDto> Submitted { get; set; } = new();
        public List<ClassworkSubmissionDto> NotSubmitted { get; set; } = new();
        [BindProperty(SupportsGet = true)] public int SubmissionPage { get; set; } = 1;
        public int SubmissionPageSize { get; set; } = 10;
        public int SubmissionsTotal => Submitted.Count + NotSubmitted.Count;

        public async Task OnGetAsync()
        {
            var facultyId = await GetFacultyIdAsync();
            await LoadHeaderDropdownsAsync();
            await LoadCoursesAsync(facultyId);
            await LoadClassworksAsync(facultyId);
            await LoadSelectedClassworkAsync(facultyId);
        }

        private async Task<Guid> GetFacultyIdAsync()
        {
            var faculty = await _facultyService.GetFacultyByAccountIdAsync(CurrentAccount.Id);
            return faculty.Id;
        }

        private async Task LoadHeaderDropdownsAsync()
        {
            SemesterOptions = await _semesterService.GetSelectOptionsAsync();
            if (!SelectedSemesterId.HasValue && SemesterOptions.Any())
                SelectedSemesterId = SemesterOptions.FirstOrDefault(s => s.Name.Contains("(Current)"))?.Id
                                     ?? SemesterOptions.First().Id;

            BatchOptions = await _batchService.GetBatchOptionsAsync();
            SectionOptions = await _sectionService.GetSectionOptionsAsync();
        }

        private async Task LoadCoursesAsync(Guid facultyId)
        {
            if (SelectedSemesterId.HasValue && SelectedBatchId.HasValue && SelectedSectionId.HasValue)
            {
                Courses = await _classworkService.GetFacultyCoursesAsync(
                    facultyId,
                    SelectedSemesterId.Value,
                    SelectedBatchId,
                    SelectedSectionId
                );

                if (SelectedCourseId.HasValue)
                    SelectedCourse = Courses.FirstOrDefault(c => c.CourseOfferingId == SelectedCourseId.Value);
            }
        }

        private async Task LoadClassworksAsync(Guid facultyId)
        {
            if (!SelectedCourseId.HasValue)
            {
                Classworks = new();
                TotalClassworks = 0;
                return;
            }

            var (items, total) = await _classworkService.GetClassworksAsync(
                facultyId,
                SelectedCourseId.Value,
                CurrentPage,
                PageSize,
                SearchTerm
            );

            Classworks = items;
            TotalClassworks = total;
        }

        private async Task LoadSelectedClassworkAsync(Guid facultyId)
        {
            if (!SelectedClassworkId.HasValue)
                return;

            SelectedClasswork = await _classworkService.GetClassworkByIdAsync(SelectedClassworkId.Value, facultyId);
            if (SelectedClasswork == null)
                return;

            // Pre-fill DTO
            ClassworkDto.Title = SelectedClasswork.Title;
            ClassworkDto.Description = SelectedClasswork.Description;
            ClassworkDto.FilePath = SelectedClasswork.FilePath;
            ClassworkDto.RequiresSubmission = SelectedClasswork.RequiresSubmission;
            ClassworkDto.DueDate = SelectedClasswork.DueDate;

            if (SelectedClasswork.RequiresSubmission)
            {
                var (submitted, notSubmitted) = await _classworkService.GetSubmissionsStatusAsync(SelectedClassworkId.Value);
                Submitted = submitted;
                NotSubmitted = notSubmitted;
            }
        }

        private async Task<string> SaveFileAsync()
        {
            if (ClassworkDto.File == null)
                return ClassworkDto.FilePath;

            return await FileHelper.SaveFileAsync(
                ClassworkDto.File,
                UploadType.Classwork,
                SelectedCourseId?.ToString()
            );
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            await R(async () =>
            {
                if (!SelectedCourseId.HasValue)
                    throw new InvalidOperationException("Please select a course before creating classwork.");

                ClassworkDto.FilePath = await SaveFileAsync();
                var facultyId = await GetFacultyIdAsync();

                await _classworkService.CreateClassworkAsync(
                    facultyId,
                    SelectedCourseId.Value,
                    ClassworkDto
                );
            }, "Classwork created successfully.");

            return RedirectToPage(new
            {
                SelectedCourseId,
                SelectedSemesterId,
                SelectedBatchId,
                SelectedSectionId,
                SearchTerm,
                CurrentPage
            });
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            await R(async () =>
            {
                if (!SelectedClassworkId.HasValue)
                    throw new InvalidOperationException("Please select a classwork before updating.");

                ClassworkDto.FilePath = await SaveFileAsync();
                var facultyId = await GetFacultyIdAsync();

                await _classworkService.UpdateClassworkAsync(
                    SelectedClassworkId.Value,
                    facultyId,
                    ClassworkDto
                );
            }, "Classwork updated successfully.");

            return RedirectToPage(new
            {
                SelectedClassworkId,
                SelectedCourseId,
                SelectedSemesterId,
                SelectedBatchId,
                SelectedSectionId,
                SearchTerm,
                CurrentPage
            });
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            await R(async () =>
            {
                if (!SelectedClassworkId.HasValue)
                    throw new InvalidOperationException("Please select a classwork before deleting.");

                await _classworkService.DeleteClassworkAsync(SelectedClassworkId.Value);
            }, "Classwork deleted successfully.");

            return RedirectToPage(new
            {
                SelectedCourseId,
                SelectedSemesterId,
                SelectedBatchId,
                SelectedSectionId,
                SearchTerm,
                CurrentPage
            });
        }
    }
}
