using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Academics.Configs
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class CourseModel : BasePageModel
    {
        private readonly CourseService _courseService;
        private readonly DepartmentService _departmentService;
        private readonly CourseTypeService _courseTypeService;

        public CourseModel(
            CourseService courseService,
            DepartmentService departmentService,
            CourseTypeService courseTypeService,
            AccountService accountService)
            : base(accountService)
        {
            _courseService = courseService;
            _departmentService = departmentService;
            _courseTypeService = courseTypeService;
        }

        public List<Course> Courses { get; set; } = new();
        public List<Department> Departments { get; set; } = new();
        public List<CourseType> CourseTypes { get; set; } = new();

        [BindProperty] public Course NewCourse { get; set; } = new();
        [BindProperty] public Course EditCourse { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string EditCourseId { get; set; }

        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            // Load dropdowns
            Departments = await _departmentService.GetAllAsync();
            CourseTypes = await _courseTypeService.GetAllAsync();

            var allCourses = await _courseService.GetAllAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allCourses = allCourses
                    .Where(c => c.Title.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                             || c.Code.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
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
                NewCourse.Code,
                NewCourse.Title,
                NewCourse.CreditHours,
                NewCourse.DepartmentId,
                NewCourse.CourseTypeId,   // <--- Important: pass CourseTypeId
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
                EditCourse = new Course
                {
                    Id = course.Id,
                    Code = course.Code,
                    Title = course.Title,
                    CreditHours = course.CreditHours,
                    DepartmentId = course.DepartmentId,
                    CourseTypeId = course.CourseTypeId   // <--- populate CourseTypeId
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
                EditCourse.Code,
                EditCourse.Title,
                EditCourse.CreditHours,
                EditCourse.DepartmentId,
                EditCourse.CourseTypeId, // <--- Important: pass CourseTypeId
                CurrentAccount.Id
            );
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            await R(() => _courseService.DeleteAsync(id, CurrentAccount.Id), "Course deleted successfully.");
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            await _courseService.ActivateAsync(Guid.Parse(id), CurrentAccount.Id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
