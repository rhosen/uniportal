using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Dtos;
using UniPortal.Dtos.Attendance;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Accounts;
using UniPortal.Services.Portals;

namespace UniPortal.Pages.Portals
{
    public class AttendanceReportModel : BasePageModel
    {
        private readonly AttendanceReportService _attendanceReportService;
        private readonly SemesterService _semesterService;
        private readonly StudentService _studentService;

        public AttendanceReportModel(
            AttendanceReportService attendanceReportService,
            SemesterService semesterService,
            StudentService studentService,
            AccountService accountService
        ) : base(accountService)
        {
            _attendanceReportService = attendanceReportService;
            _semesterService = semesterService;
            _studentService = studentService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid SelectedSemesterId { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid SelectedCourseOfferingId { get; set; }

        public List<SelectOption> SemesterOptions { get; set; } = new();
        public List<SelectOption> CourseOptions { get; set; } = new();
        public List<AttendanceReportDto> AttendanceRecords { get; set; } = new();

        public async Task OnGetAsync()
        {
            var studentId = await GetStudentId();

            SemesterOptions = await _semesterService.GetSelectOptionsAsync();

            await LoadFiltersAsync(studentId);
        }

        private async Task<Guid> GetStudentId()
        {
            var student = await _studentService.GetStudentAsync(accountId: CurrentAccount.Id);
            return student.Id;
        }

        private async Task LoadFiltersAsync(Guid studentId)
        {
            // Always load courses for the selected semester
            if (SelectedSemesterId != Guid.Empty)
            {
                CourseOptions = await _attendanceReportService.GetCourseOptionsForStudentAsync(
                    studentId, SelectedSemesterId
                );
            }

            // Load attendance only if a course is selected
            if (SelectedSemesterId != Guid.Empty && SelectedCourseOfferingId != Guid.Empty)
            {
                AttendanceRecords = await _attendanceReportService.GetAttendanceForStudentCourseAsync(
                    studentId, SelectedCourseOfferingId
                );
            }
        }

    }
}
