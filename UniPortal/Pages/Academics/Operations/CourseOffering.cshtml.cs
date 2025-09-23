using Microsoft.AspNetCore.Mvc;
using UniPortal.Dtos;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Academics.Operations;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Academics.Operations
{
    public class CourseOfferingsModel : BasePageModel
    {
        private readonly ProgramService _programService;
        private readonly BatchService _batchService;
        private readonly SectionService _sectionService;
        private readonly FacultyService _facultyService;
        private readonly RoomService _roomService;
        private readonly CourseOfferingService _courseOfferingService;
        private readonly SemesterService _semesterService;

        public CourseOfferingsModel(
            ProgramService programService,
            BatchService batchService,
            SectionService sectionService,
            FacultyService facultyService,
            RoomService roomService,
            CourseOfferingService courseOfferingService,
            AccountService accountService,
            SemesterService semesterService) : base(accountService)
        {
            _programService = programService;
            _batchService = batchService;
            _sectionService = sectionService;
            _facultyService = facultyService;
            _roomService = roomService;
            _courseOfferingService = courseOfferingService;
            _semesterService = semesterService;
        }

        [BindProperty] public Guid ProgramId { get; set; }
        [BindProperty] public Guid BatchId { get; set; }
        [BindProperty] public Guid SectionId { get; set; }
        [BindProperty] public Guid SelectedSemesterId { get; set; }
        [BindProperty] public int SemesterNumber { get; set; }
        [BindProperty] public List<CourseOfferingDto> Offerings { get; set; } = new();


        public List<SelectOption> Programs { get; set; } = new();
        public List<SelectOption> Batches { get; set; } = new();
        public List<SelectOption> Sections { get; set; } = new();
        public List<SelectOption> Faculties { get; set; } = new();
        public List<SelectOption> Rooms { get; set; } = new();
        public List<SemesterOption> SemesterNumberOptions { get; set; } = new();
        public List<SelectOption> SemesterOptions { get; set; } = new();

        public string[] Days = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        public bool IsCurrentSemester { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDropdownsAsync();
        }

        public async Task<IActionResult> OnPostLoadAsync()
        {
            await LoadDropdownsAsync();
            return Page();
        }

        private async Task LoadDropdownsAsync()
        {
            SemesterNumberOptions = _semesterService.GetSemesterNumberOptions();
            SemesterOptions = await _semesterService.GetSelectOptionsAsync();
            Programs = await _programService.GetProgramOptionsAsync();
            Batches = await _batchService.GetBatchOptionsAsync();
            Sections = await _sectionService.GetSectionOptionsAsync();
            Faculties = await _facultyService.GetFacultiesAsync(ProgramId);
            Rooms = await _roomService.GetOptionsAsync();

            if (ProgramId != Guid.Empty && SemesterNumber != 0)
            {
                Offerings = await _courseOfferingService.GetOfferingsForSemesterAsync(
                    ProgramId, BatchId, SectionId, SemesterNumber);

                IsCurrentSemester = await IsCurrentSemesterAsync(Offerings);
            }
        }


        private async Task<bool> IsCurrentSemesterAsync(List<CourseOfferingDto> offerings)
        {
            if (offerings == null || offerings.Count == 0)
                return false;

            var currentSemester = await _semesterService.GetCurrentSemesterAsync();

            if (currentSemester == null) return false;

            var existingOffering = offerings.FirstOrDefault(x => x.OfferingId != null);

            if (existingOffering == null)  return true; // new offering

            var offeringSemesterId = await _courseOfferingService.GetOfferingSemesterIdAsync(existingOffering.OfferingId.Value);
           
            return offeringSemesterId == currentSemester.Id;
        }


        public async Task<IActionResult> OnPostSaveAsync()
        {
            await R(() => _courseOfferingService.SaveOfferingsAsync(
                    Offerings, ProgramId, BatchId, SectionId, SelectedSemesterId, SemesterNumber), "Offerings saved successfully");

            await LoadDropdownsAsync();

            return RedirectToPage(new
            {
                ProgramId,
                BatchId,
                SectionId,
                SemesterNumber,
                SelectedSemesterId
            });
        }
    }
}
