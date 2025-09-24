using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using UniPortal.Dtos;
using UniPortal.Reports;
using UniPortal.Services.Academics.Operations;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Portals
{
    public class GradesModel : BasePageModel
    {
        private readonly GradeService _gradeService;
        private readonly StudentService _studentService;
        private readonly GradesReportDocument _gradesReport;

        public GradesModel(
            AccountService accountService,
            GradeService gradeService,
            StudentService studentService,
            GradesReportDocument gradesReport) // injected via DI
            : base(accountService)
        {
            _gradeService = gradeService;
            _studentService = studentService;
            _gradesReport = gradesReport;
        }

        public StudentDto Student { get; set; }
        public decimal CumulativeCGPA { get; set; }
        public Dictionary<string, List<StudenGradeDto>> GradesBySemester { get; set; }

        public async Task OnGetAsync()
        {
            await LoadStudentGradesAsync();
        }

        public async Task<IActionResult> OnGetExportPdfDirectAsync()
        {
            await LoadStudentGradesAsync();

            // Set the report data
            _gradesReport.SetData(Student, GradesBySemester);

            using var stream = new MemoryStream();
            _gradesReport.GeneratePdf(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/pdf", "Grades.pdf");
        }

        private async Task LoadStudentGradesAsync()
        {
            Student = await _studentService.GetStudentAsync(CurrentAccount.Id);

            var grades = await _gradeService.GetGradesForStudentAsync(CurrentAccount.Id);

            CumulativeCGPA = grades.Any() ? grades.Average(g => g.GPA) : 0;

            GradesBySemester = grades
                .GroupBy(g => string.IsNullOrWhiteSpace(g.SemesterName) ? "Unknown Semester" : g.SemesterName)
                .ToDictionary(g => g.Key, g => g.ToList());
        }
    }
}
