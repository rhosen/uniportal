using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Accounts;
using UniPortal.ViewModels.Users;

namespace UniPortal.Pages.Users
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class StudentModel : PageModel
    {
        private readonly StudentService _studentService;
        private readonly DepartmentService _departmentService;
        private readonly ProgramService _programService;
        private readonly SemesterService _semesterService;
        private readonly AccountService _accountService;

        public StudentModel(
            StudentService studentService,
            DepartmentService departmentService,
            ProgramService programService,
            SemesterService semesterService,
            AccountService accountService)
        {
            _studentService = studentService;
            _departmentService = departmentService;
            _programService = programService;
            _semesterService = semesterService;
            _accountService = accountService;
        }

        public List<StudentViewModel> Students { get; set; } = new();
        public List<Department> Departments { get; set; } = new();
        public List<Data.Entities.Program> Programs { get; set; } = new();
        public List<Semester> Semesters { get; set; } = new();

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
            // Fetch fully populated students from service
            var allStudents = await _studentService.GetAllOnboardedStudentsAsync();

            // Optional: search by StudentId or Email
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                allStudents = allStudents
                    .Where(s => s.StudentId.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                             || s.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Total pages for pagination
            TotalPages = (int)Math.Ceiling(allStudents.Count / (double)PageSize);

            // Paginate students
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
            Programs = await _programService.GetAllAsync();
            Semesters = await _semesterService.GetAllAsync();

            var allStudents = await _studentService.GetAllOnboardedStudentsAsync();
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
                    ProgramId = student.ProgramId,
                    DepartmentId = student.DepartmentId,
                    CurrentSemesterId = student.CurrentSemesterId,
                    Email = student.Email
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
            if (EditStudent == null || EditStudent.Id == Guid.Empty)
                return RedirectToPage(new { CurrentPage, SearchTerm });

            // Fetch the student from DB
            var student = await _studentService.GetStudentAsync(studentId: EditStudent.Id);
            if (student == null)
                return RedirectToPage(new { CurrentPage, SearchTerm });

            // Update student properties
            student.StudentId = EditStudent.StudentId?.Trim();
            student.BatchNumber = EditStudent.BatchNumber?.Trim();
            student.Section = EditStudent.Section?.Trim();
            student.ProgramId = EditStudent.ProgramId;
            student.CurrentSemesterId = EditStudent.CurrentSemesterId;

            await _studentService.CreateOrUpdateStudentAsync(student);

            // Update Email if changed
            if (!string.IsNullOrWhiteSpace(EditStudent.Email))
            {
                await _accountService.UpdateEmailAsync(student.AccountId, EditStudent.Email.Trim());
            }

            // Exit edit mode
            EditStudentId = null;

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }


        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await _studentService.DeleteAsync(Guid.Parse(id));
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
