using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using UniPortal.Constants;
using UniPortal.Dtos;
using UniPortal.Reports;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Academics.Operations;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Academics.Operations
{
    [Authorize(Roles = Roles.Faculty)]
    public class EnrollmentModel : BasePageModel
    {
        private readonly StudentService _studentService;
        private readonly EnrollmentService _enrollmentService;
        private readonly SemesterService _semesterService;
        private readonly EnrollmentReportDocument _enrollmentReport;

        public EnrollmentModel(
            StudentService studentService,
            EnrollmentService enrollmentService,
            AccountService accountService,
            SemesterService semesterService,
            EnrollmentReportDocument enrollmentReport) // injected via DI
            : base(accountService)
        {
            _studentService = studentService;
            _enrollmentService = enrollmentService;
            _semesterService = semesterService;
            _enrollmentReport = enrollmentReport;
        }

        public List<StudentDto> AllStudents { get; set; } = new();
        [BindProperty] public StudentDto SelectedStudent { get; set; }

        public List<EnrollmentCourseDto> EligibleCourses { get; set; } = new();
        public Dictionary<string, List<EnrollmentCourseDto>> PastEnrollmentsBySemester { get; set; } = new();
        public string AcademicSemesterName { get; set; } = string.Empty;

        public async Task OnGetAsync(Guid? studentId)
        {
            AllStudents = await _studentService.GetAllActiveStudentAsync();

            if (studentId.HasValue)
                await LoadSelectedStudentAsync(studentId.Value);
        }

        private async Task LoadSelectedStudentAsync(Guid studentId)
        {
            SelectedStudent = await _studentService.GetStudentByIdAsync(studentId);
            if (SelectedStudent == null) return;

            EligibleCourses = await _enrollmentService.GetEligibleWithEnrollmentStatusAsync(
                SelectedStudent.Id, SelectedStudent.CurrentSemester);

            PastEnrollmentsBySemester = await _enrollmentService.GetPastEnrollmentsGroupedBySemesterAsync(
                SelectedStudent.Id, SelectedStudent.CurrentSemester);

            var semester = await _semesterService.GetCurrentSemesterAsync();
            AcademicSemesterName = semester != null
                ? $"{semester.SemesterType} ({semester.StartDate:MMM yyyy} - {semester.EndDate:MMM yyyy})"
                : "";
        }

        public async Task<IActionResult> OnPostSaveAsync(Guid StudentId, List<Guid> EnrolledCourses)
        {
            await R(
                () => _enrollmentService.UpdateEnrollmentsAsync(
                    StudentId,
                    EnrolledCourses,
                    CurrentAccount.Id
                ),
                "Enrollments updated successfully"
            );

            // Redirect back to refresh the page
            return RedirectToPage(new { studentId = StudentId });
        }

        public async Task<IActionResult> OnGetExportEnrollmentPdfDirectAsync(Guid studentId)
        {
            var student = await _studentService.GetStudentByIdAsync(studentId);
            if (student == null) return NotFound();

            var courses = await _enrollmentService.GetEligibleWithEnrollmentStatusAsync(
                student.Id, student.CurrentSemester);

            var enrolledCourses = courses.Where(c => c.IsEnrolled).ToList();
            if (!enrolledCourses.Any()) return NotFound();

            var faculty = $"{CurrentAccount.FirstName} {CurrentAccount.LastName}";

            // Use DI-injected report and SetData()
            _enrollmentReport.SetData(student, enrolledCourses, faculty);

            using var stream = new MemoryStream();
            _enrollmentReport.GeneratePdf(stream);
            stream.Position = 0;

            var safeName = string.Concat(student.FullName.Split(Path.GetInvalidFileNameChars()));
            var fileName = $"Enrollment_{student.StudentNumber}_{safeName}.pdf";

            return File(stream.ToArray(), "application/pdf", fileName);
        }
    }
}
