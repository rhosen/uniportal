using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Services.Student;

namespace UniPortal.Pages.Admin
{
    public class AssignIdModel : PageModel
    {
        private readonly StudentService _studentService;

        public AssignIdModel(StudentService studentService)
        {
            _studentService = studentService;
        }

        public List<Data.Entities.Account> StudentsWithoutId { get; set; } = new();

        // Search & Pagination
        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        [BindProperty] public Guid AccountId { get; set; }

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
        }

        // Assign unique Student ID
        public async Task<IActionResult> OnPostAssignStudentIdAsync()
        {
            if (AccountId != Guid.Empty)
            {
                await _studentService.AssignStudentIdAsync(AccountId);
            }

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
