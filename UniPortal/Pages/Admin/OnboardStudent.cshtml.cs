using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Services.Student;
using UniPortal.Data.Entities;
using UniPortal.Services.Faculty;

namespace UniPortal.Pages.Admin
{
    public class OnboardStudentModel : PageModel
    {
        private readonly StudentService _studentService;
        private readonly DepartmentService _departmentService;

        public OnboardStudentModel(StudentService studentService,
                             DepartmentService departmentService)
        {
            _studentService = studentService;
            _departmentService = departmentService;
        }

        public List<Data.Entities.Account> StudentsWithoutId { get; set; } = new();
        public List<Department> Departments { get; set; } = new();

        // Search & Pagination
        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        // Form properties
        [BindProperty] public Guid AccountId { get; set; }
        [BindProperty] public bool UseSystemId { get; set; } = true;
        [BindProperty] public string ManualStudentId { get; set; } = string.Empty;
        [BindProperty] public string BatchNumber { get; set; } = string.Empty;
        [BindProperty] public string Section { get; set; } = string.Empty;
        [BindProperty] public Guid? DepartmentId { get; set; }

        public async Task OnGetAsync()
        {
            var allStudents = await _studentService.GetStudentsWithoutStudentIdAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allStudents = allStudents
                    .Where(s => (s.FirstName + " " + s.LastName)
                                .Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                             || s.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            TotalPages = (int)Math.Ceiling(allStudents.Count / (double)PageSize);
            StudentsWithoutId = allStudents
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            Departments = await _departmentService.GetAllAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (AccountId == Guid.Empty) return RedirectToPage();

            string studentIdToAssign = UseSystemId
                ? await _studentService.GetSystemGeneratedStudentId(AccountId)
                : ManualStudentId;

            // Save/Update student info via service
            await _studentService.CreateOrUpdateStudentAsync(new Data.Entities.Student
            {
                AccountId = AccountId,
                StudentId = studentIdToAssign,
                BatchNumber = BatchNumber,
                Section = Section,
                DepartmentId = DepartmentId
            });

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
