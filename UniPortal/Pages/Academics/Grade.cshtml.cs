using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Dtos;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Academics.Operations;
using UniPortal.Services.Accounts;
using UniPortal.ViewModels.Grades;

namespace UniPortal.Pages.Academics
{
    [Authorize(Roles = Roles.Faculty)]
    public class GradeModel : BasePageModel
    {
        private readonly GradeService _gradeService;
        private readonly SemesterService _semesterService;
        private readonly EnrollmentService _enrollmentService;

        public GradeModel(GradeService gradeService,
                          SemesterService semesterService,
                          EnrollmentService enrollmentService,
                          AccountService accountService) : base(accountService)
        {
            _gradeService = gradeService;
            _semesterService = semesterService;
            _enrollmentService = enrollmentService;
        }

        public List<TeacherGradeViewModel> Grades { get; set; } = new();
        public List<SelectOption> Semesters { get; set; } = new();
        public List<SelectOption> Students { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public Guid SelectedSemesterId { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid SelectedStudentId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            var allSemesters = await _semesterService.GetAllAsync();
            Semesters = allSemesters.Select(s => new SelectOption { Id = s.Id, Name = s.Name }).ToList();

            if (SelectedSemesterId != Guid.Empty)
            {
                var teacherId = CurrentAccount.Id;
                Students = await _enrollmentService.GetStudentsBySemesterAndTeacherAsync(SelectedSemesterId, teacherId);
            }

            // Load grades ONLY when both are selected
            if (SelectedSemesterId != Guid.Empty && SelectedStudentId != Guid.Empty)
            {
                await LoadGradesAsync();
            }
        }

        private async Task LoadGradesAsync()
        {
            var allGrades = await _gradeService.GetGradesForTeacherAsync(CurrentAccount.Id, SelectedSemesterId, SelectedStudentId);

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                allGrades = allGrades
                    .Where(g => g.SubjectName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                g.SubjectCode.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            TotalPages = (int)Math.Ceiling(allGrades.Count / (double)PageSize);
            Grades = allGrades
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostSaveAsync(Guid CourseId, decimal Marks)
        {
            await _gradeService.UpsertGradeAsync(SelectedStudentId, CourseId, Marks, CurrentAccount.Id);
            return RedirectToPage(new { SelectedSemesterId, SelectedStudentId, SearchTerm, CurrentPage });
        }
    }
}