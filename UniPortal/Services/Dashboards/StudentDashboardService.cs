using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Dtos;
using UniPortal.Services.Accounts;
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

        // -----------------------------
        // Get student profile by account ID
        // -----------------------------
        public async Task<StudentProfileViewModel> GetProfileAsync(Guid accountId)
        {
            if (accountId == Guid.Empty)
                throw new ArgumentException("Invalid account ID.", nameof(accountId));

            var query = from s in _context.Students
                        join a in _context.Accounts on s.AccountId equals a.Id
                        join p in _context.Programs on s.ProgramId equals p.Id
                        join d in _context.Departments on p.DepartmentId equals d.Id
                        join b in _context.Batches on s.BatchId equals b.Id
                        join sec in _context.Sections on s.SectionId equals sec.Id
                        where s.AccountId == accountId && !s.IsDeleted && !a.IsDeleted && a.IsActive
                        select new StudentProfileViewModel
                        {
                            FullName = $"{a.FirstName} {a.LastName}".Trim(),
                            StudentId = s.StudentNumber,
                            Program = p.Name,
                            Department = d.Name,
                            Batch = b.Number,
                            Section = sec.Name,
                            Email = a.Email,
                            Phone = a.Phone
                        };

            var profile = await query.AsNoTracking().FirstOrDefaultAsync();

            if (profile == null)
                throw new KeyNotFoundException("Student not found.");

            return profile;
        }

        // -----------------------------
        // Get dashboard metrics
        // -----------------------------
        public async Task<StudentMetricDto> GetDashboardMetricsAsync(Guid studentId)
        {
            var today = DateTime.Today;
            var currentDayOfWeek = today.DayOfWeek;

            // 1️⃣ Total courses
            var courseCount = await _context.Enrollments
                .Where(e => e.StudentId == studentId && !e.IsDeleted)
                .CountAsync();

            // 2️⃣ Pending assignments
            var pendingAssignments = await (from a in _context.Assignments
                                            join co in _context.CourseOfferings on a.CourseOfferingId equals co.Id
                                            join e in _context.Enrollments on co.Id equals e.CourseOfferingId
                                            where e.StudentId == studentId
                                                  && !a.IsDeleted && !co.IsDeleted && !e.IsDeleted
                                                  && a.DueDate >= today
                                            join s in _context.AssignmentSubmissions
                                                on new { a.Id, StudentId = studentId } equals new { Id = s.AssignmentId, StudentId = s.StudentId } into sub
                                            from submission in sub.DefaultIfEmpty()
                                            where submission == null
                                            select a.Id).CountAsync();

            // 3️⃣ Today's classes (check weekday flags)
            var todayClasses = await (from e in _context.Enrollments
                                      join co in _context.CourseOfferings on e.CourseOfferingId equals co.Id
                                      where e.StudentId == studentId && !e.IsDeleted && !co.IsDeleted
                                      && ((currentDayOfWeek == DayOfWeek.Monday && co.Mon) ||
                                          (currentDayOfWeek == DayOfWeek.Tuesday && co.Tue) ||
                                          (currentDayOfWeek == DayOfWeek.Wednesday && co.Wed) ||
                                          (currentDayOfWeek == DayOfWeek.Thursday && co.Thu) ||
                                          (currentDayOfWeek == DayOfWeek.Friday && co.Fri) ||
                                          (currentDayOfWeek == DayOfWeek.Saturday && co.Sat) ||
                                          (currentDayOfWeek == DayOfWeek.Sunday && co.Sun))
                                      select co.Id).CountAsync();

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
                ? (grades.Average() / 25).ToString("0.00") // example conversion
                : "N/A";

            // 6️⃣ Unread notifications
            var unreadNotifications = await _context.Notices
                .Where(n => !n.IsDeleted && n.TargetId == studentId.ToString())
                .CountAsync();

            // 7️⃣ Notes count
            var notesCount = await (from cm in _context.CourseMaterials
                                    join co in _context.CourseOfferings on cm.CourseOfferingId equals co.Id
                                    join e in _context.Enrollments on co.Id equals e.CourseOfferingId
                                    where e.StudentId == studentId && !cm.IsDeleted && !co.IsDeleted && !e.IsDeleted
                                    select cm.Id).CountAsync();

            // 8️⃣ Distinct classrooms
            var classroomsCount = await (from co in _context.CourseOfferings
                                         join e in _context.Enrollments on co.Id equals e.CourseOfferingId
                                         where e.StudentId == studentId && !co.IsDeleted && !e.IsDeleted
                                         select co.RoomId).Distinct().CountAsync();

            return new StudentMetricDto
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
