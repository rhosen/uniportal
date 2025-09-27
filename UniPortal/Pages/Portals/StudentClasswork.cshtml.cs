using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Dtos;
using UniPortal.Dtos.Classwork;
using UniPortal.Services.Academics.Portals;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Portals
{
    [Authorize]
    public class StudentClassworkModel : BasePageModel
    {
        private readonly StudentClassworkService _classworkService;
        private readonly StudentService _studentService;

        public StudentClassworkModel(
            StudentClassworkService classworkService,
            AccountService accountService,
            StudentService studentService)
            : base(accountService)
        {
            _classworkService = classworkService;
            _studentService = studentService;
        }

        // Dropdowns
        public List<SelectOption> SemesterOptions { get; set; } = new();
        public List<SelectOption> CourseOptions { get; set; } = new();

        // Filters
        [BindProperty(SupportsGet = true)]
        public Guid? SelectedSemesterId { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid? SelectedCourseOfferingId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        // Pagination
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        // Classworks
        public List<StudentClassworkDto> Classworks { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public Guid? SelectedClassworkId { get; set; }

        public StudentClassworkDetailsDto? SelectedClasswork { get; set; }

        // Form data for submission
        [BindProperty]
        public IFormFile? UploadFile { get; set; }

        [BindProperty]
        public string? Remarks { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDropdownsAsync();
            var studentId = await GetStudentId();

            await LoadClassworksAsync(studentId);

            if (SelectedClassworkId.HasValue)
            {
                SelectedClasswork = await _classworkService.GetClassworkDetailsAsync(
                    SelectedClassworkId.Value,
                    studentId
                );
            }
        }

        private async Task<Guid> GetStudentId()
        {
            var student = await _studentService.GetStudentAsync(accountId: CurrentAccount.Id);
            if (student == null)
                throw new InvalidOperationException("Student record not found for this account.");
            return student.Id;
        }

        public async Task<IActionResult> OnPostSubmitAsync()
        {
            if (!SelectedClassworkId.HasValue)
                return RedirectToPage();

            var studentId = await GetStudentId();

            await R(async () =>
            {
                await _classworkService.SubmitClassworkAsync(
                    SelectedClassworkId.Value,
                    studentId,
                    UploadFile!,
                    Remarks ?? string.Empty,
                    SelectedCourseOfferingId?.ToString() ?? string.Empty
                );
            }, "Submission uploaded successfully.");

            return RedirectToPage(new
            {
                SelectedSemesterId,
                SelectedCourseOfferingId,
                SearchTerm,
                CurrentPage,
                SelectedClassworkId
            });
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            if (!SelectedClassworkId.HasValue)
                return RedirectToPage();

            var studentId = await GetStudentId();

            await R(async () =>
            {
                await _classworkService.DeleteSubmissionAsync(
                    SelectedClassworkId.Value,
                    studentId
                );
            }, "Submission deleted successfully.");

            return RedirectToPage(new
            {
                SelectedSemesterId,
                SelectedCourseOfferingId,
                SearchTerm,
                CurrentPage,
                SelectedClassworkId
            });
        }

        private async Task LoadDropdownsAsync()
        {
            SemesterOptions = await _classworkService.GetSemesterOptionsAsync();

            if (SelectedSemesterId.HasValue)
            {
                CourseOptions = await _classworkService.GetCourseOptionsAsync(SelectedSemesterId.Value);
            }
        }

        private async Task LoadClassworksAsync(Guid studentId)
        {
            if (!SelectedSemesterId.HasValue || !SelectedCourseOfferingId.HasValue)
                return;

            var (classworks, totalCount) = await _classworkService.GetClassworkListAsync(
                studentId, // pass actual student ID
                SelectedSemesterId,
                SelectedCourseOfferingId,
                SearchTerm,
                CurrentPage,
                PageSize
            );

            Classworks = classworks;
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
        }
    }
}
