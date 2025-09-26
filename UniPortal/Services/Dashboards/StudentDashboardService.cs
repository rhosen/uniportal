using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Dtos;
using UniPortal.ViewModels.Users;

namespace UniPortal.Services.Dashboards
{
    public class StudentDashboardService
    {
        private readonly UniPortalContext _context;

        public StudentDashboardService(UniPortalContext context)
        {
            _context = context;
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
                            Batch = b.Name,
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

            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == studentId);

            if (student == null)
                throw new KeyNotFoundException("Student not found.");

            // -----------------------------
            // Total courses
            // -----------------------------
            var courseCount = await _context.Enrollments
                .Where(e => e.StudentId == studentId && !e.IsDeleted)
                .CountAsync();

            // -----------------------------
            // Pending classwork (requires submission & not submitted)
            // -----------------------------
            var pendingClasswork = await (from cw in _context.Classworks
                                          join co in _context.CourseOfferings on cw.CourseOfferingId equals co.Id
                                          join e in _context.Enrollments on co.Id equals e.CourseOfferingId
                                          where e.StudentId == studentId
                                                && !cw.IsDeleted && !co.IsDeleted && !e.IsDeleted
                                                && cw.RequiresSubmission
                                                && cw.DueDate >= today
                                          join sub in _context.ClassworkSubmissions
                                              on new { ClassworkId = cw.Id, StudentId = studentId }
                                              equals new { sub.ClassworkId, sub.StudentId } into subGroup
                                          from submission in subGroup.DefaultIfEmpty()
                                          where submission == null
                                          select cw.Id).CountAsync();

            // -----------------------------
            // Today's classes
            // -----------------------------
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

            // -----------------------------
            // Attendance %
            // -----------------------------
            var totalClasses = await _context.Attendances
                .Where(a => a.StudentId == studentId && !a.IsDeleted)
                .CountAsync();

            var presentCount = await _context.Attendances
                .Where(a => a.StudentId == studentId && !a.IsDeleted && a.Status == "Present")
                .CountAsync();

            int attendancePercent = totalClasses > 0
                ? (int)Math.Round((double)presentCount / totalClasses * 100)
                : 0;

            // -----------------------------
            // Overall GPA
            // -----------------------------
            var gpaList = await _context.Grades
                .Where(g => g.StudentId == studentId && !g.IsDeleted)
                .Select(g => g.GPA)
                .ToListAsync();

            string overallGPA = gpaList.Any()
                ? gpaList.Average().ToString("0.00")
                : "N/A";

            // -----------------------------
            // Unread notifications
            // -----------------------------
            var unreadNotifications = await _context.Notifications
                .Where(n => !n.IsDeleted && n.AccountId == student.AccountId)
                .Where(n => !_context.NotificationReads
                              .Any(nr => nr.NotificationId == n.Id && nr.AccountId == student.AccountId && nr.IsRead))
                .CountAsync();

            // -----------------------------
            // Classwork count (notes)
            // -----------------------------
            var classworkCount = await (from cw in _context.Classworks
                                        join co in _context.CourseOfferings on cw.CourseOfferingId equals co.Id
                                        join e in _context.Enrollments on co.Id equals e.CourseOfferingId
                                        where e.StudentId == studentId && !cw.IsDeleted && !co.IsDeleted && !e.IsDeleted
                                        select cw.Id).CountAsync();

            // -----------------------------
            // Distinct classrooms
            // -----------------------------
            var classroomsCount = await (from co in _context.CourseOfferings
                                         join e in _context.Enrollments on co.Id equals e.CourseOfferingId
                                         where e.StudentId == studentId && !co.IsDeleted && !e.IsDeleted
                                         select co.RoomId).Distinct().CountAsync();

            return new StudentMetricDto
            {
                Courses = courseCount,
                PendingAssignments = pendingClasswork,
                TodayClasses = todayClasses,
                AttendancePercent = attendancePercent,
                OverallGPA = overallGPA,
                UnreadNotifications = unreadNotifications,
                NotesCount = classworkCount,
                Classrooms = classroomsCount
            };
        }
    }
}
