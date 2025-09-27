using Microsoft.AspNetCore.Mvc;
using UniPortal.Dtos;
using UniPortal.Dtos.Schedule;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Academics.Portals;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Portals
{
    public class ScheduleModel : BasePageModel
    {
        private readonly StudentScheduleService _scheduleService;
        private readonly SemesterService _semesterService;
        private readonly StudentService _studentService;

        public ScheduleModel(
            StudentScheduleService scheduleService,
            SemesterService semesterService,
            AccountService accountService,
            StudentService studentService) : base(accountService)
        {
            _scheduleService = scheduleService;
            _semesterService = semesterService;
            _studentService = studentService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid SelectedSemesterId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? WeekStart { get; set; }

        public List<SelectOption> SemesterOptions { get; set; } = new();
        public List<StudentScheduleCourseDto> Courses { get; set; } = new();
        public List<StudentScheduleTimeSlotDto> TimeSlots { get; set; } = new();
        public string[] DaysOfWeek { get; } = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        public List<DateTime> WeekDates { get; set; } = new();

        public async Task OnGet()
        {
            // Load semester dropdown
            SemesterOptions = await _semesterService.GetSelectOptionsAsync();
            var currentSemester = await _semesterService.GetCurrentSemesterAsync();
            SelectedSemesterId = SelectedSemesterId == Guid.Empty ? currentSemester.Id : SelectedSemesterId;

            // Load current student
            var student = await _studentService.GetStudentAsync(accountId: CurrentAccount.Id);

            // Load courses
            Courses = _scheduleService.GetStudentCourses(student.Id, SelectedSemesterId);

            // Generate hourly slots
            TimeSlots = Enumerable.Range(8, 11)
                .Select(h => new StudentScheduleTimeSlotDto { Hour = h })
                .ToList();

            // Compute week dates (Mon-Sun)
            DateTime monday;
            if (WeekStart.HasValue)
            {
                monday = WeekStart.Value;
            }
            else
            {
                var today = DateTime.Today;
                var diff = today.DayOfWeek - DayOfWeek.Monday;
                if (diff < 0) diff += 7;
                monday = today.AddDays(-diff);
            }

            WeekDates = Enumerable.Range(0, 7)
                .Select(i => monday.AddDays(i))
                .ToList();
        }

        public StudentScheduleCourseDto? GetCourseAt(string day, TimeSpan startTime) =>
            Courses.FirstOrDefault(c =>
                c.Days.Contains(day) &&
                c.StartTime == startTime);
    }
}
