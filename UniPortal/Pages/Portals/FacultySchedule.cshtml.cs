using Microsoft.AspNetCore.Mvc;
using UniPortal.Dtos;
using UniPortal.Dtos.Schedule;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Portals
{
    public class FacultyScheduleModel : BasePageModel
    {
        private readonly FacultyScheduleService _scheduleService;
        private readonly SemesterService _semesterService;
        private readonly FacultyService _facultyService;

        public FacultyScheduleModel(
            FacultyScheduleService scheduleService,
            SemesterService semesterService,
            FacultyService facultyService,
            AccountService accountService) : base(accountService)
        {
            _scheduleService = scheduleService;
            _semesterService = semesterService;
            _facultyService = facultyService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid SelectedSemesterId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? WeekStart { get; set; }

        public List<SelectOption> SemesterOptions { get; set; } = new();
        public List<FacultyScheduleCourseDto> Courses { get; set; } = new();
        public List<StudentScheduleTimeSlotDto> TimeSlots { get; set; } = new();
        public List<DateTime> WeekDates { get; set; } = new();

        [BindProperty]
        public Guid CancelCourseOfferingId { get; set; }

        [BindProperty]
        public DateTime CancelDate { get; set; }

        [BindProperty]
        public string? CancelReason { get; set; }

        public async Task OnGet()
        {
            SemesterOptions = await _semesterService.GetSelectOptionsAsync();
            var currentSemester = await _semesterService.GetCurrentSemesterAsync();
            SelectedSemesterId = SelectedSemesterId == Guid.Empty ? currentSemester.Id : SelectedSemesterId;

            var semester = await _semesterService.GetByIdAsync(SelectedSemesterId.ToString());

            var today = DateTime.Today;
            var diff = today.DayOfWeek - DayOfWeek.Monday;
            if (diff < 0) diff += 7;
            var monday = WeekStart ?? today.AddDays(-diff);

            WeekDates = Enumerable.Range(0, 7).Select(i => monday.AddDays(i)).ToList();

            TimeSlots = Enumerable.Range(8, 11).Select(h => new StudentScheduleTimeSlotDto { Hour = h }).ToList();

            var faculty = await _facultyService.GetFacultyByAccountIdAsync(CurrentAccount.Id);

            Courses = _scheduleService.GetFacultyCourses(faculty.Id, SelectedSemesterId, WeekDates);
        }

        public async Task<IActionResult> OnPostCancelAsync()
        {
            var faculty = await _facultyService.GetFacultyByAccountIdAsync(CurrentAccount.Id);
            await _scheduleService.CancelClassAsync(faculty.Id, CancelCourseOfferingId, CancelDate, CancelReason);
            return RedirectToPage(new { SelectedSemesterId, WeekStart });
        }

        // Calendar helper: get courses for a specific date and time slot
        public List<FacultyScheduleCourseDto> GetCoursesAt(DateTime date, TimeSpan slotStart)
        {
            return Courses
                .Where(c =>
                    c.Date.Date == date.Date &&
                    c.StartTime < slotStart + TimeSpan.FromHours(1) && // overlap with the slot
                    c.EndTime > slotStart
                )
                .ToList();
        }
    }
}
