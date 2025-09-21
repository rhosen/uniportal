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
        private readonly SectionService _sectionService;
        private readonly BatchService _batchService;

        public OnboardModel(StudentService studentService, 
                            ProgramService programService,
                            SectionService sectionService,
                            BatchService batchService)
        {
            _studentService = studentService;
            _programService = programService;
            _sectionService = sectionService;
            _batchService = batchService;
        }

        public List<StudentOnboardDto> Students { get; set; } = new();
        public List<Data.Entities.Program> Programs { get; set; } = new();
        public List<SelectOption> Batches { get; set; } = new();
        public List<SelectOption> Sections { get; set; } = new();

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

            // Use existing method to get Batches and Sections
            Batches = await _batchService.GetBatchOptionsAsync();
            Sections = await _sectionService.GetSectionOptionsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Student.ProgramId == Guid.Empty)
            {
                ModelState.AddModelError("Student.ProgramId", "Program is required.");
                await OnGetAsync();
                return Page();
            }

            if (!Student.UseSystemId && string.IsNullOrWhiteSpace(Student.StudentNumber))
            {
                ModelState.AddModelError("Student.StudentId", "Student ID is required when using manual ID.");
                await OnGetAsync();
                return Page();
            }

            if (Student.BatchId == Guid.Empty)
            {
                ModelState.AddModelError("Student.BatchId", "Batch is required.");
                await OnGetAsync();
                return Page();
            }

            if (Student.SectionId == Guid.Empty)
            {
                ModelState.AddModelError("Student.SectionId", "Section is required.");
                await OnGetAsync();
                return Page();
            }

            string studentIdToAssign = Student.UseSystemId
                ? await _studentService.GetSystemGeneratedStudentId(Student.AccountId)
                : Student.StudentNumber;

            await _studentService.CreateOrUpdateStudentAsync(new Student
            {
                AccountId = Student.AccountId,
                StudentNumber = studentIdToAssign,
                BatchId = Student.BatchId,
                SectionId = Student.SectionId,
                ProgramId = Student.ProgramId
            });

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
