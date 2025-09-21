using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Dtos;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Academics.Configs
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class DepartmentsModel : BasePageModel
    {
        private readonly DepartmentService _departmentService;
        private readonly FacultyService _facultyService;

        public DepartmentsModel(DepartmentService departmentService,
                                FacultyService facultyService,
                                AccountService accountService) : base(accountService)
        {
            _departmentService = departmentService;
            _facultyService = facultyService;
        }

        // Display table
        public List<Data.Entities.Department> Departments { get; set; } = new();

        // Dropdowns
        public List<SelectOption> HeadOptions { get; set; } = new();

        // Form bindings
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
            // Load faculty options for Head dropdown
            HeadOptions = await _facultyService.GetSelectOptionsAsync();

            var allDepartments = await _departmentService.GetAllAsync();

            // Search filter
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allDepartments = allDepartments
                    .Where(d => d.Code.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Pagination
            TotalPages = (int)Math.Ceiling(allDepartments.Count / (double)PageSize);
            Departments = allDepartments
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            await _departmentService.CreateAsync(NewDepartment.Code, NewDepartment.Name);
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
            await _departmentService.UpdateAsync(Guid.Parse(id), EditDepartment.Code, EditDepartment.Name);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }


        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await D(() => _departmentService.DeleteAsync(id), "Department deleted successfully.");
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            await _departmentService.ActivateAsync(id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
