using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Dtos;
using UniPortal.Dtos.Attendance;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Academics.Portals;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Portals
{
    public class AttendanceModel : BasePageModel
    {
        private readonly AttendanceService _attendanceService;
        private readonly SemesterService _semesterService;
        private readonly BatchService _batchService;
        private readonly SectionService _sectionService;
        private readonly FacultyService _facultyService;

        public AttendanceModel(
            AttendanceService attendanceService,
            SemesterService semesterService,
            BatchService batchService,
            SectionService sectionService,
            FacultyService facultyService,
            AccountService accountService
        ) : base(accountService)
        {
            _attendanceService = attendanceService;
            _semesterService = semesterService;
            _batchService = batchService;
            _sectionService = sectionService;
            _facultyService = facultyService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid SelectedSemesterId { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid SelectedBatchId { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid SelectedSectionId { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid SelectedCourseOfferingId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime SelectedDate { get; set; } = DateTime.Today;

        public AttendanceViewDto AttendanceView { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StudentSearchTerm { get; set; } = "";

        public List<AttendanceRowDto> FilteredStudents => string.IsNullOrWhiteSpace(StudentSearchTerm)
            ? AttendanceView?.Students ?? new List<AttendanceRowDto>()
            : AttendanceView?.Students
                .Where(s => s.StudentName.Contains(StudentSearchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList() ?? new List<AttendanceRowDto>();

        public List<SelectOption> SemesterOptions { get; set; } = new();
        public List<SelectOption> BatchOptions { get; set; } = new();
        public List<SelectOption> SectionOptions { get; set; } = new();
        public List<SelectOption> CourseOptions { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadFiltersAsync();

            if (SelectedCourseOfferingId != Guid.Empty)
            {
                AttendanceView = await _attendanceService.GetAttendanceForDateAsync(
                    SelectedCourseOfferingId, SelectedDate
                );
            }
        }

        private async Task LoadFiltersAsync()
        {
            SemesterOptions = await _semesterService.GetSelectOptionsAsync();
            BatchOptions = await _batchService.GetBatchOptionsAsync();
            SectionOptions = await _sectionService.GetSectionOptionsAsync();

            var faculty = await _facultyService.GetFacultyByAccountIdAsync(CurrentAccount.Id);

            if (SelectedSemesterId != Guid.Empty && SelectedBatchId != Guid.Empty && SelectedSectionId != Guid.Empty)
            {
                CourseOptions = await _attendanceService.GetCourseOptionsAsync(
                    faculty.Id, SelectedSemesterId, SelectedBatchId, SelectedSectionId
                );
            }
        }

        public async Task<IActionResult> OnPostSaveAttendanceAsync()
        {
            await LoadFiltersAsync();

            if (SelectedCourseOfferingId != Guid.Empty)
            {
                AttendanceView = await _attendanceService.GetAttendanceForDateAsync(
                    SelectedCourseOfferingId, SelectedDate
                );

                if (AttendanceView != null && !AttendanceView.IsCanceled && SelectedDate >= DateTime.Today)
                {
                    // Map individual form values
                    foreach (var student in AttendanceView.Students)
                    {
                        var statusField = Request.Form[$"Status_{student.StudentId}"].ToString();
                        var remarksField = Request.Form[$"Remarks_{student.StudentId}"].ToString();

                        if (!string.IsNullOrEmpty(statusField))
                            student.Status = statusField;

                        student.Remarks = remarksField;
                    }

                    await R(() => _attendanceService.SaveAttendanceAsync(
                        AttendanceView.CourseOfferingId,
                        AttendanceView.Date,
                        AttendanceView.Students
                    ), "Attendance saved successfully!");
                }
            }

            return Page();
        }
    }
}
