using Microsoft.AspNetCore.Mvc;
using UniPortal.Dtos;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Academics.Operations;
using UniPortal.Services.Accounts;
using UniPortal.ViewModels.Operations;

namespace UniPortal.Pages.Academics.Operations
{
    public class CourseOfferingsModel : BasePageModel
    {
        private readonly ProgramService _programService;
        private readonly BatchService _batchService;
        private readonly SectionService _sectionService;
        private readonly FacultyService _facultyService;
        private readonly CurriculumService _curriculumService;
        private readonly CourseOfferingService _courseOfferingService;
        private readonly IConfiguration _configuration;

        public CourseOfferingsModel(
            ProgramService programService,
            BatchService batchService,
            SectionService sectionService,
            FacultyService facultyService,
            CurriculumService curriculumService,
            CourseOfferingService courseOfferingService,
            IConfiguration configuration,
            AccountService accountService) : base(accountService)
        {
            _programService = programService;
            _batchService = batchService;
            _sectionService = sectionService;
            _facultyService = facultyService;
            _curriculumService = curriculumService;
            _courseOfferingService = courseOfferingService;
            _configuration = configuration;
        }

        // Dropdowns
        public List<SelectOption> Programs { get; set; } = new();
        public List<SelectOption> Batches { get; set; } = new();
        public List<SelectOption> Sections { get; set; } = new();
        public List<SelectOption> Faculties { get; set; } = new();

        // Semester dropdown configurable via appsettings
        public List<SelectOption> Semesters { get; set; } = new();

        // Unified model for binding form
        [BindProperty]
        public CourseOfferingForm Form { get; set; } = new();

        // Existing offerings for editing
        public List<CourseOfferingDto> ExistingOfferings { get; set; } = new();

        // Courses from curriculum to add
        public List<CourseOfferingDto> CurriculumCourses { get; set; } = new();

        public async Task OnGetAsync()
        {
            Programs = await _programService.GetProgramOptionsAsync();
            Batches = await _batchService.GetBatchOptionsAsync();
            Sections = await _sectionService.GetSectionOptionsAsync();
            Faculties = new List<SelectOption>();

            // Load semesters from appsettings (e.g., "SemesterCount":8)
            int semesterCount = _configuration.GetValue<int>("SemesterCount", 8);
            Semesters = Enumerable.Range(1, semesterCount)
                .Select(n => new SelectOption { Id = Guid.Empty, Name = n.ToString() })
                .ToList();
        }

        public async Task<IActionResult> OnPostLoadCoursesAsync()
        {
            if (Form.ProgramId == Guid.Empty || Form.SemesterNumber == 0)
                return BadRequest("Program and Semester must be selected.");

            // Load curriculum courses (for adding)
            CurriculumCourses = await _curriculumService.GetCurriculumCoursesAsync(Form.ProgramId, Form.SemesterNumber);

            // Load faculties for dropdown
            Faculties = await _facultyService.GetFacultiesAsync(Form.ProgramId);

            // Load existing offerings for editing
            ExistingOfferings = await _courseOfferingService.GetOfferingsAsync(Form.ProgramId, Form.SemesterNumber, Form.BatchId, Form.SectionId);

            // Remove from CurriculumCourses those already added
            var existingCourseIds = ExistingOfferings.Select(e => e.CourseId).ToHashSet();
            CurriculumCourses = CurriculumCourses.Where(c => !existingCourseIds.Contains(c.CourseId)).ToList();

            // Reload other dropdowns
            Programs = await _programService.GetProgramOptionsAsync();
            Batches = await _batchService.GetBatchOptionsAsync();
            Sections = await _sectionService.GetSectionOptionsAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostSaveOfferingsAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Add or update offerings (both new from curriculum and edited existing)
            await _courseOfferingService.AddOrUpdateOfferingsAsync(
                Form.ProgramId,
                Form.SemesterNumber,
                Form.BatchId,
                Form.SectionId,
                Form.Courses);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteOfferingAsync(Guid id)
        {
            if (id == Guid.Empty) return BadRequest();
            await D(() => _courseOfferingService.DeleteAsync(id), "Course offering deleted successfully.");
            return RedirectToPage();
        }
    }

}
