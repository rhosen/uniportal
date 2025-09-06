using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Services;
using UniPortal.Services.Faculty;

namespace UniPortal.Pages.Admin
{
    public class CourseModel : BasePageModel
    {
        private readonly CourseService _courseService;
        private readonly DepartmentService _departmentService;
        private readonly TeacherService _teacherService;
        private readonly SemesterService _semesterService;
        private readonly SubjectService _subjectService;

        public CourseModel(
            CourseService courseService,
            DepartmentService departmentService,
            TeacherService teacherService,
            SemesterService semesterService,
            SubjectService subjectService,
            AccountService accountService): base(accountService)
        {
            _courseService = courseService;
            _departmentService = departmentService;
            _teacherService = teacherService;
            _semesterService = semesterService;
            _subjectService = subjectService;
        }

        public List<Data.Entities.Course> Courses { get; set; } = new();
        public List<Data.Entities.Department> Departments { get; set; } = new();
        public List<Data.Entities.Account> Teachers { get; set; } = new();
        public List<Data.Entities.Semester> Semesters { get; set; } = new();
        public List<Data.Entities.Subject> Subjects { get; set; } = new();

        [BindProperty] public Data.Entities.Course NewCourse { get; set; } = new();
        [BindProperty] public Data.Entities.Course EditCourse { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string EditCourseId { get; set; }

        // Search & Pagination
        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            Departments = await _departmentService.GetAllAsync();
            Teachers = await _teacherService.GetAllAsync();
            Semesters = await _semesterService.GetAllAsync();
            Subjects = await _subjectService.GetAllAsync();

            var allCourses = await _courseService.GetAllAsync();
            // ? should include Subject, Department, Teacher, Semester for display

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allCourses = allCourses
                    .Where(c =>
                        c.Subject != null &&
                        (c.Subject.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                         c.Subject.Code.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            TotalPages = (int)Math.Ceiling(allCourses.Count / (double)PageSize);
            Courses = allCourses
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            await _courseService.CreateAsync(
                NewCourse.SubjectId,
                NewCourse.DepartmentId,
                NewCourse.TeacherId,
                NewCourse.SemesterId,
                NewCourse.Credits,
                CurrentAccount.Id
            );
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditCourseId = id;
            var course = await _courseService.GetByIdAsync(Guid.Parse(id));
            if (course != null)
            {
                EditCourse = new Data.Entities.Course
                {
                    Id = course.Id,
                    SubjectId = course.SubjectId,
                    DepartmentId = course.DepartmentId,
                    TeacherId = course.TeacherId,
                    SemesterId = course.SemesterId,
                    Credits = course.Credits
                };
            }
            await OnGetAsync();
            return Page();
        }

        public IActionResult OnPostCancelEdit()
        {
            EditCourseId = null;
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostSaveEditAsync(string id)
        {
            await _courseService.UpdateAsync(
                Guid.Parse(id),
                EditCourse.SubjectId,
                EditCourse.DepartmentId,
                EditCourse.TeacherId,
                EditCourse.SemesterId,
                EditCourse.Credits,
                CurrentAccount.Id
            );
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await _courseService.DeleteAsync(Guid.Parse(id), CurrentAccount.Id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            await _courseService.ActivateAsync(Guid.Parse(id), CurrentAccount.Id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
