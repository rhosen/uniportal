using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Constants;
using UniPortal.Dtos;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Accounts;
using UniPortal.ViewModels.Users;
using static UniPortal.Constants.AppConstant;

namespace UniPortal.Pages.Users
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class FacultyModel : PageModel
    {
        private readonly FacultyService _facultyService;
        private readonly DepartmentService _departmentService;

        public FacultyModel(FacultyService facultyService, 
                            DepartmentService departmentService)
        {
            _facultyService = facultyService;
            _departmentService = departmentService;
        }

        // Display table
        public List<FacultyViewModel> Faculties { get; set; } = new();

        // Form bindings
        [BindProperty] public FacultyViewModel NewFaculty { get; set; } = new();
        [BindProperty] public FacultyViewModel EditFaculty { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string EditFacultyId { get; set; }

        // Dropdowns
        public List<SelectOption> Departments { get; set; } = new();

        // Pagination & Search
        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            // Load department options for dropdowns
            Departments = await _departmentService.GetDepartmentOptionsAsync();

            var allFaculties = await _facultyService.GetAllAsync();

            // Search filter
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allFaculties = allFaculties
                    .Where(f => f.FirstName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                             || f.LastName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                             || f.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Pagination
            TotalPages = (int)Math.Ceiling(allFaculties.Count / (double)PageSize);
            Faculties = allFaculties
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid) return Page();

            var password = Passwords.Faculty;

            // DepartmentId must be set in NewFaculty
            await _facultyService.CreateAsync(
                NewFaculty.Email,
                password,
                NewFaculty.FirstName,
                NewFaculty.LastName,
                NewFaculty.DepartmentId
            );

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditFacultyId = id;
            var faculty = await _facultyService.GetAsync(accountId: Guid.Parse(id));
            if (faculty != null)
            {
                EditFaculty = faculty;
            }

            await OnGetAsync();
            return Page();
        }

        public IActionResult OnPostCancelEdit()
        {
            EditFacultyId = null;
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostSaveEditAsync(string id)
        {
            if (!ModelState.IsValid) return Page();

            EditFaculty.AccountId = Guid.Parse(id);

            // Simply delegate everything to the service
            await _facultyService.UpdateAsync(EditFaculty);

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }


        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await _facultyService.DeleteAsync(Guid.Parse(id));
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            await _facultyService.ActivateAsync(Guid.Parse(id));
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
