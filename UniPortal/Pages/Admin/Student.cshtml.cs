using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using UniPortal.Services;
using UniPortal.Services.Faculty;
using UniPortal.Services.Student;
using UniPortal.ViewModel;

namespace UniPortal.Pages.Admin
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class StudentModel : PageModel
    {
        private readonly StudentService _studentService;
        private readonly DepartmentService _departmentService;
        private readonly AccountService _accountService;

        public StudentModel(StudentService studentService, DepartmentService departmentService, AccountService accountService)
        {
            _studentService = studentService;
            _departmentService = departmentService;
            _accountService = accountService;
        }

        public List<StudentViewModel> Students { get; set; } = new();
        public List<Department> Departments { get; set; } = new();

        // Pagination & Search
        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        // Track which row is in edit mode
        [BindProperty(SupportsGet = true)]
        public string EditStudentId { get; set; }

        // Temp storage for edited student
        [BindProperty] public StudentViewModel EditStudent { get; set; } = new();

        public async Task OnGetAsync()
        {
            Departments = await _departmentService.GetAllAsync();
            var allStudents = await _studentService.GetAllOnboardedStudentAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allStudents = allStudents
                    .Where(s => s.StudentId.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                             || s.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            TotalPages = (int)Math.Ceiling(allStudents.Count / (double)PageSize);
            Students = allStudents
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        // Enter edit mode
        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditStudentId = id;

            Departments = await _departmentService.GetAllAsync();
            var allStudents = await _studentService.GetAllOnboardedStudentAsync();

            TotalPages = (int)Math.Ceiling(allStudents.Count / (double)PageSize);
            Students = allStudents
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var student = allStudents.FirstOrDefault(s => s.Id.ToString() == id);
            if (student != null)
            {
                EditStudent = new StudentViewModel
                {
                    Id = student.Id,
                    StudentId = student.StudentId,
                    BatchNumber = student.BatchNumber,
                    Section = student.Section,
                    DepartmentId = student.DepartmentId,
                    Email = student.Email,
                };
            }

            return Page();
        }

        // Cancel edit mode
        public IActionResult OnPostCancelEdit()
        {
            EditStudentId = null;
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        // Save changes
        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (EditStudent == null) return RedirectToPage(new { CurrentPage, SearchTerm });

            var student = await _studentService.GetByIdAsync(EditStudent.Id);
            if (student != null)
            {
                student.StudentId = EditStudent.StudentId;
                student.BatchNumber = EditStudent.BatchNumber;
                student.Section = EditStudent.Section;
                student.DepartmentId = EditStudent.DepartmentId;

                await _studentService.CreateOrUpdateStudentAsync(student);

                // Update Email
                if (!string.IsNullOrEmpty(EditStudent.Email))
                {
                    await _accountService.UpdateEmailAsync(student.AccountId, EditStudent.Email);
                }
            }

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
