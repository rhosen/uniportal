using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniPortal.Data;

namespace UniPortal.Services.Faculty
{
    public class FacultyDashboardService
    {
        private readonly UniPortalContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public FacultyDashboardService(UniPortalContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Total courses assigned to this teacher
        public async Task<int> GetTotalCoursesAsync(Guid teacherAccountId)
        {
            return await _context.Courses
                .Where(c => c.TeacherId == teacherAccountId && !c.IsDeleted)
                .CountAsync();
        }

        // Total students enrolled in this teacher's courses
        public async Task<int> GetTotalStudentsAsync(Guid teacherAccountId)
        {
            return await _context.Enrollments
                .Where(e => !e.IsDeleted && _context.Courses
                    .Any(c => c.Id == e.CourseId && c.TeacherId == teacherAccountId))
                .Select(e => e.StudentId)
                .Distinct()
                .CountAsync();
        }

        // Assignments created by this teacher
        public async Task<int> GetTotalAssignmentsAsync(Guid teacherAccountId)
        {
            return await _context.Assignments
                .Where(a => !a.IsDeleted && _context.Courses
                    .Any(c => c.Id == a.CourseId && c.TeacherId == teacherAccountId))
                .CountAsync();
        }

        // Submissions received for this teacher's assignments
        public async Task<int> GetTotalSubmissionsAsync(Guid teacherAccountId)
        {
            return await _context.AssignmentSubmissions
                .Where(s => !s.IsDeleted && _context.Assignments
                    .Any(a => a.Id == s.AssignmentId && _context.Courses
                        .Any(c => c.Id == a.CourseId && c.TeacherId == teacherAccountId)))
                .CountAsync();
        }

        // Class notes uploaded by this teacher
        public async Task<int> GetTotalNotesAsync(Guid teacherAccountId)
        {
            return await _context.ClassNotes
                .Where(n => !n.IsDeleted && n.TeacherId == teacherAccountId)
                .CountAsync();
        }

        // Pending attendance for this teacher's classes
        public async Task<int> GetPendingAttendanceAsync(Guid teacherAccountId)
        {
            return await _context.Attendance
                .Where(a => !a.IsDeleted && _context.ClassSchedules
                    .Any(cs => cs.Id == a.ClassScheduleId && cs.TeacherId == teacherAccountId))
                .CountAsync();
        }

        // Recent notifications created by this teacher (last month)
        public async Task<int> GetRecentNotificationsAsync(Guid teacherAccountId)
        {
            var oneMonthAgo = DateTime.Now.AddMonths(-1);

            return await _context.Notifications
                .Where(n => !n.IsDeleted && n.CreatedBy == teacherAccountId && n.CreatedAt >= oneMonthAgo)
                .CountAsync();
        }

        // Coordinator: total enrollments for courses they manage
        public async Task<int> GetTotalEnrollmentsAsync(Guid coordinatorId)
        {
            // Assuming coordinator manages courses where they are assigned as Teacher
            var courseIds = await _context.Courses
                .Where(c => c.TeacherId == coordinatorId && !c.IsDeleted)
                .Select(c => c.Id)
                .ToListAsync();

            return await _context.Enrollments
                .Where(e => courseIds.Contains(e.CourseId) && !e.IsDeleted)
                .CountAsync();
        }

        // Coordinator: total scheduled classes for courses they manage
        public async Task<int> GetTotalScheduledClassesAsync(Guid coordinatorId)
        {
            var courseIds = await _context.Courses
                .Where(c => c.TeacherId == coordinatorId && !c.IsDeleted)
                .Select(c => c.Id)
                .ToListAsync();

            return await _context.ClassSchedules
                .Where(cs => courseIds.Contains(cs.CourseId) && !cs.IsDeleted)
                .CountAsync();
        }
    }
}
