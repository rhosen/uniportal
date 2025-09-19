using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Services.Accounts;
using UniPortal.ViewModels.Dashboards;
using UniPortal.ViewModels.Users;

namespace UniPortal.Services.Dashboards
{
    public class StudentDashboardService
    {
        private readonly UniPortalContext _context;
        private readonly StudentService _studentService;

        public StudentDashboardService(
            UniPortalContext context,
            StudentService studentService)
        {
            _context = context;
            _studentService = studentService;
        }

        public async Task<StudentProfileViewModel> GetProfileAsync(Guid accountId)
        {
            if (accountId == Guid.Empty)
                throw new ArgumentException("Invalid account ID.", nameof(accountId));

            var query = from s in _context.Students
                        join a in _context.Accounts on s.AccountId equals a.Id
                        join p in _context.Programs on s.ProgramId equals p.Id
                        join d in _context.Departments on p.DepartmentId equals d.Id
                        where s.AccountId == accountId && !s.IsDeleted && !a.IsDeleted && a.IsActive
                        select new StudentProfileViewModel
                        {
                            FullName = $"{a.FirstName} {a.LastName}".Trim(),
                            StudentId = s.StudentId,
                            Program = p.Name,
                            Department = d.Name,
                            Batch = s.BatchNumber,
                            Section = s.Section,
                            Email = a.Email,
                            Phone = a.Phone
                        };

            var profile = await query.AsNoTracking().FirstOrDefaultAsync();

            if (profile == null)
                throw new KeyNotFoundException("Student not found.");

            return profile;
        }



        // Dashboard metrics
        public async Task<MetricsViewModel> GetDashboardMetricsAsync(Guid studentId)
        {
            var today = DateTime.Today;

            // 1️⃣ Courses
            var courseCount = await _context.Enrollments
                .CountAsync(e => e.StudentId == studentId && !e.IsDeleted);

            // 2️⃣ Pending Assignments
            var pendingAssignments = await _context.Assignments
                .Where(a => !a.IsDeleted && a.DueDate >= today)
                .CountAsync(a => !_context.Submissions
                    .Any(s => s.AssignmentId == a.Id && s.StudentId == studentId));

            // 3️⃣ Today's Classes
            var currentDay = today.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)today.DayOfWeek;

            var todayClasses = await _context.Sessions
                .Where(e => !e.IsDeleted &&
                            !e.Schedule.IsDeleted &&
                            e.Schedule.Course.Enrollments.Any(en => en.StudentId == studentId) &&
                            e.DayOfWeek == currentDay)
                .CountAsync();

            // 4️⃣ Attendance %
            var totalClasses = await _context.Attendances
                .Where(a => a.StudentId == studentId && !a.IsDeleted)
                .CountAsync();

            var presentCount = await _context.Attendances
                .Where(a => a.StudentId == studentId && !a.IsDeleted && a.Status == "Present")
                .CountAsync();

            int attendancePercent = totalClasses > 0
                ? (int)Math.Round((double)presentCount / totalClasses * 100)
                : 0;

            // 5️⃣ Overall GPA
            var grades = await _context.Grades
                .Where(g => g.StudentId == studentId && !g.IsDeleted)
                .Select(g => g.Marks)
                .ToListAsync();

            string overallGPA = grades.Any()
                ? (grades.Average() / 25).ToString("0.00") // Example: convert marks to GPA (0-4 scale)
                : "N/A";

            // 6️⃣ Unread Notifications
            var unreadNotifications = await _context.Notices
                .CountAsync(n => !n.IsDeleted && n.StudentId == studentId.ToString());

            // 7️⃣ Notes Count
            var notesCount = await _context.Notes
                .CountAsync(n => !n.IsDeleted && n.Course.Enrollments.Any(e => e.StudentId == studentId));

            // 8️⃣ Classrooms count (optional: number of distinct classrooms student has today)
            var classroomsCount = await _context.Schedules
                .Where(c => !c.IsDeleted && c.Course.Enrollments.Any(e => e.StudentId == studentId))
                .Select(c => c.RoomId)
                .Distinct()
                .CountAsync();

            return new MetricsViewModel
            {
                Courses = courseCount,
                PendingAssignments = pendingAssignments,
                TodayClasses = todayClasses,
                AttendancePercent = attendancePercent,
                OverallGPA = overallGPA,
                UnreadNotifications = unreadNotifications,
                NotesCount = notesCount,
                Classrooms = classroomsCount
            };
        }

    }
}
