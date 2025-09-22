using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Constants;
using UniPortal.Dtos;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Accounts;
using UniPortal.ViewModels.Users;

namespace UniPortal.Pages.Users
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class StudentModel : PageModel
    {
        private readonly StudentService _studentService;
        private readonly ProgramService _programService;
        private readonly BatchService _batchService;
        private readonly SectionService _sectionService;
        private readonly AccountService _accountService;
        private readonly SemesterService _semesterService;

        public StudentModel(
            StudentService studentService,
            ProgramService programService,
            BatchService batchService,
            SectionService sectionService,
            AccountService accountService,
            SemesterService semesterService)
        {
            _studentService = studentService;
            _programService = programService;
            _batchService = batchService;
            _sectionService = sectionService;
            _accountService = accountService;
            _semesterService = semesterService;
        }

        public List<StudentViewModel> Students { get; set; } = new();
        public List<SelectOption> Programs { get; set; } = new();
        public List<SelectOption> Batches { get; set; } = new();
        public List<SelectOption> Sections { get; set; } = new();
        public List<SemesterOption> SemesterNumberOptions { get; set; } = new(); 

        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        [BindProperty(SupportsGet = true)] public string EditStudentId { get; set; }
        [BindProperty] public StudentViewModel EditStudent { get; set; } = new();

        public async Task OnGetAsync()
        {
            Programs = await _programService.GetProgramOptionsAsync();
            Batches = await _batchService.GetBatchOptionsAsync();
            Sections = await _sectionService.GetSectionOptionsAsync();
            SemesterNumberOptions = _semesterService.GetSemesterNumberOptions();

            var allStudents = await _studentService.GetAllOnboardedStudentsAsync();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
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


        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditStudentId = id;
            await OnGetAsync();

            var student = Students.FirstOrDefault(s => s.Id.ToString() == id);
            if (student != null)
            {
                EditStudent = new StudentViewModel
                {
                    Id = student.Id,
                    StudentId = student.StudentId,
                    Email = student.Email,
                    BatchId = student.BatchId,       // Save ID
                    SectionId = student.SectionId,   // Save ID
                    ProgramId = student.ProgramId,   // Save ID
                    CurrentSemester = student.CurrentSemester
                };
            }

            return Page();
        }

        public IActionResult OnPostCancelEdit()
        {
            EditStudentId = null;
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (EditStudent == null || EditStudent.Id == Guid.Empty)
                return RedirectToPage(new { CurrentPage, SearchTerm });

            var student = await _studentService.GetStudentAsync(studentId: EditStudent.Id);
            if (student == null)
                return RedirectToPage(new { CurrentPage, SearchTerm });

            // Map ViewModel -> Entity
            student.StudentNumber = EditStudent.StudentId?.Trim();
            student.BatchId = EditStudent.BatchId;
            student.SectionId = EditStudent.SectionId;
            student.ProgramId = EditStudent.ProgramId;
            student.CurrentSemester = EditStudent.CurrentSemester;

            await _studentService.CreateOrUpdateStudentAsync(student);

            if (!string.IsNullOrWhiteSpace(EditStudent.Email) && EditStudent.Email != student.Account.Email)
            {
                await _accountService.UpdateEmailAsync(student.AccountId, EditStudent.Email.Trim());
            }

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
