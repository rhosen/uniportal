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

        // Remove fixed DaysOfWeek
        public string[] DaysOfWeek { get; set; } = Array.Empty<string>();

        public List<DateTime> WeekDates { get; set; } = new();

        public async Task OnGet()
        {
            // Load semester dropdown
            SemesterOptions = await _semesterService.GetSelectOptionsAsync();
            var currentSemester = await _semesterService.GetCurrentSemesterAsync();
            SelectedSemesterId = SelectedSemesterId == Guid.Empty ? currentSemester.Id : SelectedSemesterId;

            // Load semester info
            var semester = await _semesterService.GetByIdAsync(SelectedSemesterId.ToString());

            // Compute week dates dynamically (Mon-Sun)
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

            // Dynamic day names for headers
            DaysOfWeek = WeekDates.Select(d => d.ToString("ddd")).ToArray();

            // Only load courses if the week is inside the semester
            if (WeekDates.First() >= semester.StartDate && WeekDates.Last() <= semester.EndDate)
            {
                var student = await _studentService.GetStudentAsync(accountId: CurrentAccount.Id);
                Courses = _scheduleService.GetStudentCourses(student.Id, SelectedSemesterId);
            }
            else
            {
                Courses = new List<StudentScheduleCourseDto>();
            }

            // Generate hourly slots (8AM to 18PM)
            TimeSlots = Enumerable.Range(8, 11)
                .Select(h => new StudentScheduleTimeSlotDto { Hour = h })
                .ToList();
        }

        public StudentScheduleCourseDto? GetCourseAt(string day, TimeSpan startTime) =>
            Courses.FirstOrDefault(c =>
                c.Days.Contains(day) &&
                c.StartTime == startTime);
    }
}
