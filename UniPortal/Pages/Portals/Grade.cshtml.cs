using ClosedXML.Excel;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Dtos;
using UniPortal.Helpers;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Academics.Operations;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Portals
{
    public class GradesModel : BasePageModel
    {
        private readonly GradeService _gradeService;
        private readonly IConverter _pdfConverter;
        private readonly StudentService _studentService;
        private readonly DepartmentService _departmentService;
        private readonly RazorViewToStringRenderer _razorRenderer;

        public GradesModel(
            AccountService accountService,
            GradeService gradeService,
            IConverter pdfConverter,
            StudentService studentService,
            DepartmentService departmentService,
            RazorViewToStringRenderer razorRenderer)
            : base(accountService)
        {
            _gradeService = gradeService;
            _pdfConverter = pdfConverter;
            _studentService = studentService;
            _departmentService = departmentService;
            this._razorRenderer = razorRenderer;
        }

        public string StudentName { get; set; }
        public string StudentId { get; set; }
        public string Program { get; set; }
        public decimal CumulativeCGPA { get; set; }
        public Dictionary<string, List<StudenGradeDto>> GradesBySemester { get; set; }

        public async Task OnGetAsync()
        {
            var grades = await _gradeService.GetGradesForStudentAsync(CurrentAccount.Id);
            var student = await _studentService.GetStudentAsync(CurrentAccount.Id);
            var department = await _departmentService.GetByIdAsync(student.ProgramId.ToString());

            StudentName = CurrentAccount.FirstName + " " + CurrentAccount.LastName;
            StudentId = student.StudentNumber;
            Program = department.Name;
            CumulativeCGPA = grades.Any() ? grades.Average(g => g.GPA) : 0;

            GradesBySemester = grades
                .GroupBy(g => string.IsNullOrWhiteSpace(g.SemesterName) ? "Unknown Semester" : g.SemesterName)
                .ToDictionary(g => g.Key, g => g.ToList());

        }

        // Excel Export
        public async Task<IActionResult> OnGetExportExcelAsync()
        {
            var grades = await _gradeService.GetGradesForStudentAsync(CurrentAccount.Id);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Grades");

            ws.Cell(1, 1).Value = "Semester";
            ws.Cell(1, 2).Value = "Subject Code";
            ws.Cell(1, 3).Value = "Subject Name";
            ws.Cell(1, 4).Value = "Teacher";
            ws.Cell(1, 5).Value = "Grade";
            ws.Cell(1, 6).Value = "Marks";
            ws.Cell(1, 7).Value = "GPA";

            int row = 2;
            foreach (var g in grades)
            {
                ws.Cell(row, 1).Value = g.SemesterName;
                ws.Cell(row, 2).Value = g.SubjectCode;
                ws.Cell(row, 3).Value = g.SubjectName;
                ws.Cell(row, 4).Value = g.TeacherName;
                ws.Cell(row, 5).Value = g.Grade;
                ws.Cell(row, 6).Value = g.Marks;
                ws.Cell(row, 7).Value = g.GPA;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "Grades.xlsx");
        }

        // PDF Export
        public async Task<IActionResult> OnGetExportPdfAsync()
        {
            var html = await _razorRenderer.RenderViewAsync("/Pages/Portals/Grades.cshtml", this);
            var doc = new HtmlToPdfDocument
            {
                GlobalSettings = { PaperSize = PaperKind.A4, Orientation = Orientation.Portrait },
                Objects = { new ObjectSettings { HtmlContent = html } }
            };
            var pdf = _pdfConverter.Convert(doc);
            return File(pdf, "application/pdf", "Grades.pdf");
        }
    }
}
