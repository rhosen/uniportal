using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using UniPortal.Dtos;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Academics.Operations;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Academics
{
    [Authorize(Roles = Roles.Faculty)]
    public class EnrollmentModel : BasePageModel
    {
        private readonly EnrollmentService _enrollmentService;
        private readonly CourseService _courseService;
        private readonly StudentService _studentService;
        private readonly SemesterService _semesterService;

        public EnrollmentModel(
            EnrollmentService enrollmentService,
            CourseService courseService,
            StudentService studentService,
            SemesterService semesterService,
            AccountService accountService): base(accountService)
        {
            _enrollmentService = enrollmentService;
            _courseService = courseService;
            _studentService = studentService;
            _semesterService = semesterService;
        }

        // Grid & Add
        public List<EnrollmentDto> Enrollments { get; set; } = new();
        [BindProperty] public NewEnrollmentInput NewEnrollment { get; set; } = new();

        // Select lists
        public List<SelectListItem> StudentSelectList { get; set; } = new();
        public List<SelectListItem> CourseSelectList { get; set; } = new();
        public List<CourseDto> CourseDetails { get; set; } = new();

        // Semester info
        public Semester CurrentSemester { get; set; }

        // Search & pagination
        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            CurrentSemester = await _semesterService.GetCurrentSemesterAsync();

            // New: all courses in current semester
            var courses = await _courseService.GetCoursesBySemesterAsync(CurrentSemester.Id);
            CourseSelectList = courses.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.DepartmentCode + " - " + c.SubjectCode + " - " + c.SubjectName }).ToList();

            CourseDetails = courses.Select(c => new CourseDto
            {
                Id = c.Id,
                DepartmentName = c.DepartmentName,
                TeacherName = c.TeacherName,
                Credits = c.Credits
            }).ToList();

            var students = await _studentService.GetAllActiveStudentAsync();
            StudentSelectList = students.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.StudentId + " - " + s.FullName }).ToList();

            var allEnrollments = await _enrollmentService.GetEnrollmentsForSemesterAsync(CurrentSemester.Id);

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allEnrollments = allEnrollments
                    .Where(e => e.StudentName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                             || e.StudentId.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            TotalPages = (int)Math.Ceiling(allEnrollments.Count / (double)PageSize);
            Enrollments = allEnrollments
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            CurrentSemester = await _semesterService.GetCurrentSemesterAsync();
            await _enrollmentService.EnrollStudentAsync(NewEnrollment.StudentId, NewEnrollment.CourseId, CurrentSemester.Id, CurrentAccount.Id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await _enrollmentService.DeleteEnrollmentAsync(id, CurrentAccount.Id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            await _enrollmentService.ActivateEnrollmentAsync(id, CurrentAccount.Id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public class NewEnrollmentInput
        {
            public Guid StudentId { get; set; }
            public Guid CourseId { get; set; }
        }
    }
}
