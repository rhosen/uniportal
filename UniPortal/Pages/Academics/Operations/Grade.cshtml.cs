using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Dtos;
using UniPortal.Services.Academics.Operations;
using UniPortal.Services.Accounts;
using UniPortal.ViewModels.Grades;
using UniPortal.Services.Academics.Configs;

namespace UniPortal.Pages.Academics.Operations
{
    [Authorize(Roles = Roles.Faculty)]
    public class GradeModel : BasePageModel
    {
        private readonly GradeService _gradeService;
        private readonly EnrollmentService _enrollmentService;
        private readonly SemesterService _semesterService;

        public GradeModel(
            GradeService gradeService,
            EnrollmentService enrollmentService,
            AccountService accountService,
            SemesterService semesterService
        ) : base(accountService)
        {
            _gradeService = gradeService;
            _enrollmentService = enrollmentService;
            _semesterService = semesterService;
        }

        public List<FacultyGradeDto> Grades { get; set; } = new();
        public List<SemesterOption> SemesterNumberOptions { get; set; } = new();
        public List<SelectOption> Students { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int SelectedSemesterNumber { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid SelectedStudentId { get; set; }

        public async Task OnGetAsync()
        {
            SemesterNumberOptions = _semesterService.GetSemesterNumberOptions();

            if (SelectedSemesterNumber > 0)
                Students = await _gradeService.GetStudentsToGradeAsync(SelectedSemesterNumber, CurrentAccount.Id);

            if (SelectedSemesterNumber > 0 && SelectedStudentId != Guid.Empty)
                Grades = await _gradeService.GetGradesByFacultyAsync(CurrentAccount.Id, SelectedSemesterNumber, SelectedStudentId);
        }


        public async Task<IActionResult> OnPostSaveAsync(Guid CourseOfferingId, decimal Marks)
        {
            await R(()=> _gradeService.UpsertGradeAsync(SelectedStudentId, CourseOfferingId, Marks, CurrentAccount.Id), "Grades updated successfully!");

            return RedirectToPage(new
            {
                SelectedSemesterNumber,
                SelectedStudentId
            });
        }
    }
}
