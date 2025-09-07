using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Services;
using UniPortal.Services.Faculty;

namespace UniPortal.Pages.Admin
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class ClassScheduleModel : BasePageModel
    {
        private readonly ClassScheduleService _scheduleService;
        private readonly CourseService _courseService;
        private readonly ClassroomService _classroomService;

        public ClassScheduleModel(
            ClassScheduleService scheduleService,
            CourseService courseService,
            ClassroomService classroomService,
            AccountService accountService) : base(accountService)
        {
            _scheduleService = scheduleService;
            _courseService = courseService;
            _classroomService = classroomService;
        }

        public List<Data.Entities.ClassSchedule> Schedules { get; set; } = new();
        public List<Data.Entities.Course> Courses { get; set; } = new();
        public List<Data.Entities.Classroom> Classrooms { get; set; } = new();

        [BindProperty] public Data.Entities.ClassSchedule NewSchedule { get; set; } = new();
        [BindProperty] public Data.Entities.ClassSchedule EditSchedule { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string EditScheduleId { get; set; }

        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            Courses = await _courseService.GetOngoingCoursesAsync();
            Classrooms = await _classroomService.GetAllAsync();

            var allSchedules = await _scheduleService.GetAllAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allSchedules = allSchedules
                    .Where(s =>
                        s.Course.Subject.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        s.Course.Subject.Code.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (s.Course.Teacher.FirstName + " " + s.Course.Teacher.LastName)
                            .Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            TotalPages = (int)Math.Ceiling(allSchedules.Count / (double)PageSize);
            Schedules = allSchedules
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            await _scheduleService.CreateAsync(NewSchedule.CourseId, NewSchedule.ClassroomId, NewSchedule.DayOfWeek, NewSchedule.StartTime, NewSchedule.EndTime, CurrentAccount.Id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditScheduleId = id;
            var sched = await _scheduleService.GetByIdAsync(id);
            if (sched != null)
            {
                EditSchedule = new Data.Entities.ClassSchedule
                {
                    Id = sched.Id,
                    CourseId = sched.CourseId,
                    ClassroomId = sched.ClassroomId,
                    DayOfWeek = sched.DayOfWeek,
                    StartTime = sched.StartTime,
                    EndTime = sched.EndTime
                };
            }
            await OnGetAsync();
            return Page();
        }

        public IActionResult OnPostCancelEdit()
        {
            EditScheduleId = null;
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostSaveEditAsync(string id)
        {
            await _scheduleService.UpdateAsync(id, EditSchedule.CourseId, EditSchedule.ClassroomId, EditSchedule.DayOfWeek, EditSchedule.StartTime, EditSchedule.EndTime, CurrentAccount.Id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await _scheduleService.DeleteAsync(id, CurrentAccount.Id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
