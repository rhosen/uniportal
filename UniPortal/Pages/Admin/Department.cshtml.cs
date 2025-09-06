using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Services.Faculty;

namespace UniPortal.Pages.Admin
{
    public class DepartmentsModel : PageModel
    {
        private readonly DepartmentService _departmentService;
        private readonly TeacherService _teacherService;

        public DepartmentsModel(DepartmentService departmentService, TeacherService teacherService)
        {
            _departmentService = departmentService;
            _teacherService = teacherService;
        }

        public List<Data.Entities.Department> Departments { get; set; } = new();
        public List<Data.Entities.Account> Teachers { get; set; } = new();

        [BindProperty] public Data.Entities.Department NewDepartment { get; set; } = new();
        [BindProperty] public Data.Entities.Department EditDepartment { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string EditDepartmentId { get; set; }

        // Search & Pagination
        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            Teachers = await _teacherService.GetAllAsync(); // To populate Head dropdown
            var allDepartments = await _departmentService.GetAllAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allDepartments = allDepartments
                    .Where(d => d.Code.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            TotalPages = (int)Math.Ceiling(allDepartments.Count / (double)PageSize);
            Departments = allDepartments
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            await _departmentService.CreateAsync(NewDepartment.Code, NewDepartment.Name, NewDepartment.HeadId);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditDepartmentId = id;
            var dept = await _departmentService.GetByIdAsync(id);
            if (dept != null)
            {
                EditDepartment = new Data.Entities.Department
                {
                    Id = dept.Id,
                    Code = dept.Code,
                    Name = dept.Name,
                    HeadId = dept.HeadId
                };
            }
            await OnGetAsync();
            return Page();
        }

        public IActionResult OnPostCancelEdit()
        {
            EditDepartmentId = null;
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostSaveEditAsync(string id)
        {
            await _departmentService.UpdateAsync(Guid.Parse(id), EditDepartment.Code, EditDepartment.Name, EditDepartment.HeadId);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await _departmentService.DeleteAsync(id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            await _departmentService.ActivateAsync(id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
