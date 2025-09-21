using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniPortal.Constants;
using UniPortal.Dtos;
using UniPortal.Services.Academics.Operations;
using UniPortal.Services.Accounts;
using UniPortal.ViewModels.Grades;

namespace UniPortal.Pages.Academics.Operations
{
    [Authorize(Roles = Roles.Faculty)]
    public class GradeModel : BasePageModel
    {
        private readonly GradeService _gradeService;
        private readonly EnrollmentService _enrollmentService;
        private readonly IConfiguration _configuration;

        public GradeModel(GradeService gradeService,
                          EnrollmentService enrollmentService,
                          AccountService accountService,
                          IConfiguration configuration) : base(accountService)
        {
            _gradeService = gradeService;
            _enrollmentService = enrollmentService;
            _configuration = configuration;
        }

        public List<TeacherGradeViewModel> Grades { get; set; } = new();
        public List<SemesterOption> Semesters { get; set; } = new();
        public List<SelectOption> Students { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int SelectedSemesterNumber { get; set; }

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
            int totalSemesters = _configuration.GetValue("TotalSemesters", 8);

            // Generate semesters dropdown
            Semesters = Enumerable.Range(1, totalSemesters)
                                  .Select(n => new SemesterOption { Number = n, Name = n.ToString() })
                                  .ToList();

            if (SelectedSemesterNumber > 0)
            {
                var teacherId = CurrentAccount.Id;
                Students = await _enrollmentService.GetStudentsBySemesterAndTeacherAsync(SelectedSemesterNumber, teacherId);
            }

            if (SelectedSemesterNumber > 0 && SelectedStudentId != Guid.Empty)
            {
                await LoadGradesAsync();
            }
        }

        private async Task LoadGradesAsync()
        {
            var allGrades = await _gradeService.GetGradesForTeacherAsync(CurrentAccount.Id, SelectedSemesterNumber, SelectedStudentId);

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
            return RedirectToPage(new
            {
                SelectedSemesterNumber,
                SelectedStudentId,
                SearchTerm,
                CurrentPage
            });
        }
    }
}
