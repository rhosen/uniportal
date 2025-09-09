using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Dtos;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Academics.Operations;
using UniPortal.Services.Accounts;
using UniPortal.ViewModels;
using UniPortal.ViewModels.Classes;

namespace UniPortal.Pages.Classes
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class ScheduleModel : BasePageModel
    {
        private readonly ClassScheduleService _scheduleService;

        public ScheduleModel(
            ClassScheduleService scheduleService,
            AccountService accountService) : base(accountService)
        {
            _scheduleService = scheduleService;
        }

        // ======================
        // Display Schedules
        // ======================
        public List<ScheduleViewModel> Schedules { get; set; } = new();

        // ======================
        // Dropdowns
        // ======================
        public List<SelectOption> Courses { get; set; } = new();
        public List<SelectOption> Classrooms { get; set; } = new();

        // ======================
        // Input binding for create/edit
        // ======================
        [BindProperty]
        public ScheduleInputModel Schedule { get; set; } = new();

        // ======================
        // Pagination & search
        // ======================
        [BindProperty(SupportsGet = true)]
        public PageInfo PageInfo { get; set; } = new();


        [BindProperty(SupportsGet = true)]
        public Guid? EditScheduleId { get; set; }

        // ======================
        // OnGet
        // ======================
        public async Task OnGetAsync()
        {
            Courses = await _scheduleService.GetCoursesForDropdownAsync();
            Classrooms = await _scheduleService.GetClassroomsForDropdownAsync();
            Schedules = await _scheduleService.GetSchedulesAsync(PageInfo.SearchTerm);

            // Pagination
            PageInfo.TotalPages = (int)Math.Ceiling(Schedules.Count / (double)PageInfo.PageSize);
            Schedules = Schedules
                .Skip((PageInfo.CurrentPage - 1) * PageInfo.PageSize)
                .Take(PageInfo.PageSize)
                .ToList();

            // Load schedule into top form if editing
            if (EditScheduleId.HasValue)
            {
                await LoadSelectedScheduleAsync();
            }
        }

        private async Task LoadSelectedScheduleAsync()
        {

            var sched = await _scheduleService.GetScheduleByIdAsync(EditScheduleId.Value);
            if (sched == null) return;


            Schedule = new ScheduleInputModel
            {
                ScheduleId = sched.ScheduleId,
                SelectedCourseId = sched.SelectedCourseId,
                SelectedClassroomId = sched.SelectedClassroomId,
                StartTime = sched.StartTime,
                EndTime = sched.EndTime,
                SelectedDays = sched.SelectedDays
            };
        }



        // ======================
        // Create multiple entries
        // ======================
        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (Schedule.SelectedCourseId == Guid.Empty ||
                Schedule.SelectedClassroomId == Guid.Empty ||
                !Schedule.SelectedDays.Any())
                return Page();

            await _scheduleService.CreateScheduleAsync(
                Schedule.SelectedCourseId,
                Schedule.SelectedClassroomId,
                Schedule.SelectedDays,
                Schedule.StartTime,
                Schedule.EndTime,
                CurrentAccount.Id
            );

            return RedirectToPage(new { PageInfo.CurrentPage, PageInfo.SearchTerm });
        }

        public IActionResult OnPostEdit(Guid scheduleId)
        {
            EditScheduleId = scheduleId;
            return RedirectToPage(new { EditScheduleId = scheduleId, PageInfo.CurrentPage, PageInfo.SearchTerm });
        }

        // ======================
        // Edit single entry
        // ======================
        public async Task<IActionResult> OnPostSaveEditAsync()
        {
            await _scheduleService.UpdateScheduleAsync(
                Schedule.ScheduleId,
                Schedule.SelectedCourseId,
                Schedule.SelectedClassroomId,
                Schedule.SelectedDays,
                Schedule.StartTime,
                Schedule.EndTime,
                CurrentAccount.Id
            );

            return RedirectToPage(new { PageInfo.CurrentPage, PageInfo.SearchTerm });
        }

        // ======================
        // Delete
        // ======================
        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await _scheduleService.DeleteScheduleAsync(Guid.Parse(id), CurrentAccount.Id);
            return RedirectToPage(new { PageInfo.CurrentPage, PageInfo.SearchTerm });
        }

        // ======================
        // Cancel Edit
        // ======================
        public IActionResult OnPostCancelEdit()
        {
            return RedirectToPage(new { PageInfo.CurrentPage, PageInfo.SearchTerm });
        }
    }
}
