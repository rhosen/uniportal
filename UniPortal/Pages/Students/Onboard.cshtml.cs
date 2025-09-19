using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Data.Entities;
using UniPortal.Dtos;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Students
{
    public class OnboardModel : PageModel
    {
        private readonly StudentService _studentService;
        private readonly ProgramService _programService;

        public OnboardModel(StudentService studentService, ProgramService programService)
        {
            _studentService = studentService;
            _programService = programService;
        }

        public List<StudentOnboardDto> Students { get; set; } = new();
        public List<Data.Entities.Program> Programs { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        [BindProperty]
        public StudentOnboardDto Student { get; set; } = new();

        public async Task OnGetAsync()
        {
            var allStudents = await _studentService.GetStudentsWithoutStudentIdAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allStudents = allStudents
                    .Where(s => s.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            TotalPages = (int)Math.Ceiling(allStudents.Count / (double)PageSize);
            Students = allStudents
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            Programs = await _programService.GetAllAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Student.ProgramId == Guid.Empty || string.IsNullOrWhiteSpace(Student.ProgramId.ToString()))
            {
                ModelState.AddModelError("Student.ProgramId", "Program is required.");
                await OnGetAsync();
                return Page();
            }

            if (!Student.UseSystemId && string.IsNullOrWhiteSpace(Student.StudentId))
            {
                ModelState.AddModelError("Student.StudentId", "Student ID is required when using manual ID.");
                await OnGetAsync();
                return Page();
            }

            string studentIdToAssign = Student.UseSystemId
                ? await _studentService.GetSystemGeneratedStudentId(Student.AccountId)
                : Student.StudentId;

            await _studentService.CreateOrUpdateStudentAsync(new Student
            {
                AccountId = Student.AccountId,
                StudentId = studentIdToAssign,
                BatchNumber = Student.BatchNumber,
                Section = Student.Section,
                ProgramId = Student.ProgramId
            });

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
