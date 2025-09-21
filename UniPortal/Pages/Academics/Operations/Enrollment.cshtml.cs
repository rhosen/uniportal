using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Dtos;
using UniPortal.Services.Academics;
using UniPortal.Services.Academics.Operations;
using UniPortal.Services.Accounts;
namespace UniPortal.Pages.Academics.Operations
{
    [Authorize(Roles = Roles.Faculty)]
    public class EnrollmentModel : BasePageModel
    {
        private readonly StudentService _studentService;
        private readonly CourseOfferingService _courseOfferingService;
        private readonly EnrollmentService _enrollmentService;

        public EnrollmentModel(
            StudentService studentService,
            CourseOfferingService courseService,
            EnrollmentService enrollmentService,
            AccountService accountService) : base(accountService)
        {
            _studentService = studentService;
            _courseOfferingService = courseService;
            _enrollmentService = enrollmentService;
        }

        // Students
        public List<StudentDto> AllStudents { get; set; } = new();
        [BindProperty] public StudentDto SelectedStudent { get; set; }

        // Current semester courses
        public List<CourseDto> EligibleCourses { get; set; } = new();

        // Past enrollments
        public Dictionary<string, List<CourseDto>> PastEnrollmentsBySemester { get; set; } = new();

        // GET: load all students and optionally selected student
        public async Task OnGetAsync(Guid? studentId)
        {
            AllStudents = await _studentService.GetAllActiveStudentAsync();

            if (studentId.HasValue)
            {
                await LoadSelectedStudentAsync(studentId.Value);
            }
        }

        private async Task LoadSelectedStudentAsync(Guid studentId)
        {
            SelectedStudent = await _studentService.GetStudentByIdAsync(studentId);
            if (SelectedStudent == null) return;

            // Current semester courses
            EligibleCourses = await _courseOfferingService.GetEligibleCoursesForStudentAsync(SelectedStudent.Id, SelectedStudent.CurrentSemester);

            // Mark enrolled courses
            var enrolledCourses = await _enrollmentService.GetEnrollmentsAsync(SelectedStudent.Id, SelectedStudent.CurrentSemester);
            foreach (var c in EligibleCourses)
            {
                c.IsEnrolled = enrolledCourses.Any(e => e.CourseId == c.Id);
            }

            // Past enrollments
            PastEnrollmentsBySemester = await _enrollmentService.GetPastEnrollmentsGroupedBySemesterAsync(
                SelectedStudent.Id, SelectedStudent.CurrentSemester);
        }

        // POST: save toggled enrollments
        public async Task<IActionResult> OnPostSaveAsync(Guid StudentId, List<Guid> EnrolledCourses)
        {
            SelectedStudent = await _studentService.GetStudentByIdAsync(StudentId);

            await _enrollmentService.UpdateEnrollmentsAsync(StudentId, EnrolledCourses, SelectedStudent.CurrentSemester, CurrentAccount.Id);

            // Redirect to GET to refresh page with updated info
            return RedirectToPage(new { studentId = StudentId });
        }
    }
}