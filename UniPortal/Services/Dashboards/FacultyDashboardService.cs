using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.ViewModels.Dashboards;
using UniPortal.ViewModels.Users;

namespace UniPortal.Services.Dashboards
{
    public class FacultyDashboardService
    {
        private readonly UniPortalContext _context;

        public FacultyDashboardService(UniPortalContext context)
        {
            _context = context;
        }

        // -----------------------------
        // Get faculty profile by account ID
        // -----------------------------
        public async Task<FacultyProfileViewModel> GetFacultyProfileAsync(Guid accountId)
        {
            return await _context.Accounts
                .Where(a => a.Id == accountId && !a.IsDeleted)
                .Select(a => new FacultyProfileViewModel
                {
                    Id = a.Id,
                    FullName = a.FirstName + " " + a.LastName,
                    Email = a.Email,
                    Phone = a.Phone
                })
                .FirstOrDefaultAsync();
        }

        // -----------------------------
        // Get dashboard metrics
        // -----------------------------
        public async Task<FacultyMetricsViewModel> GetDashboardMetricsAsync(Guid accountId)
        {
            // Total courses taught by faculty
            int totalCourses = await _context.Courses
                .Where(c => c.TeacherId == accountId && !c.IsDeleted)
                .CountAsync();

            // Upcoming class (next scheduled)
            var now = DateTime.Now;
            var currentDay = ((int)now.DayOfWeek == 0) ? 7 : (int)now.DayOfWeek; // Sunday = 0 → 7
            var currentTime = TimeOnly.FromDateTime(now);

            var nextClassEntry = await _context.ClassScheduleEntries
                .Include(e => e.Schedule)
                    .ThenInclude(s => s.Course)
                        .ThenInclude(c => c.Subject)
                .Where(e => !e.IsDeleted &&
                            e.Schedule.Course.TeacherId == accountId)
                .OrderBy(e => e.DayOfWeek)    // Optional: order by day
                .ThenBy(e => e.StartTime)
                .FirstOrDefaultAsync();

            string nextClass = nextClassEntry != null
                ? $"{nextClassEntry.StartTime:hh\\:mm} - {nextClassEntry.Schedule.Course.Subject.Name}"
                : "N/A";

            return new FacultyMetricsViewModel
            {
                TotalCourses = totalCourses,
                UpcomingClass = nextClass
            };
        }

    }
}
