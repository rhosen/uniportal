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
            var student = await _studentService.GetStudentAsync(accountId);

            if (student == null || student.Account == null)
                throw new Exception("Student not found.");

            return new StudentProfileViewModel
            {
                FullName = $"{student.Account.FirstName} {student.Account.LastName}".Trim(),
                StudentId = student.StudentId, // human-readable
                Department = student.Department?.Name ?? "",
                Batch = student.BatchNumber ?? "",
                Section = student.Section ?? "",
                Email = student.Account.Email,
                Phone = student.Account.Phone ?? ""
            };
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
                .CountAsync(a => !_context.AssignmentSubmissions
                    .Any(s => s.AssignmentId == a.Id && s.StudentId == studentId));

            // 3️⃣ Today's Classes
            var todayClasses = await _context.ClassSchedules
                .Where(c => !c.IsDeleted &&
                            c.Course.Enrollments.Any(e => e.StudentId == studentId) &&
                            c.DayOfWeek == (int)today.DayOfWeek)
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
                .Where(g => g.StudentId == studentId && !g.IsDeleted && g.Marks.HasValue)
                .Select(g => g.Marks.Value)
                .ToListAsync();

            string overallGPA = grades.Any()
                ? (grades.Average() / 25).ToString("0.00") // Example: convert marks to GPA (0-4 scale)
                : "N/A";

            // 6️⃣ Unread Notifications
            var unreadNotifications = await _context.Notices
                .CountAsync(n => !n.IsDeleted && n.RecipientId == studentId.ToString());

            // 7️⃣ Notes Count
            var notesCount = await _context.ClassNotes
                .CountAsync(n => !n.IsDeleted && n.Course.Enrollments.Any(e => e.StudentId == studentId));

            // 8️⃣ Classrooms count (optional: number of distinct classrooms student has today)
            var classroomsCount = await _context.ClassSchedules
                .Where(c => !c.IsDeleted && c.Course.Enrollments.Any(e => e.StudentId == studentId))
                .Select(c => c.ClassroomId)
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
