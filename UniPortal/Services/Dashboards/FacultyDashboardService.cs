using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Dtos;
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
            var query = from f in _context.Faculties
                        join a in _context.Accounts on f.AccountId equals a.Id
                        join ft in _context.FacultyTypes on f.FacultyTypeId equals ft.Id
                        where f.AccountId == accountId && !f.IsDeleted && !a.IsDeleted
                        select new FacultyProfileViewModel
                        {
                            Id = a.Id,
                            FullName = a.FirstName + " " + a.LastName,
                            Email = a.Email,
                            Phone = a.Phone,
                            IsAdvisor = f.IsAdvisor,
                            Title = ft.Name
                        };

            return await query.FirstOrDefaultAsync();
        }

        // -----------------------------
        // Get dashboard metrics
        // -----------------------------
        public async Task<FacultyMetricDto> GetDashboardMetricsAsync(Guid accountId)
        {
            // -----------------------------
            // Get faculty ID
            // -----------------------------
            var facultyId = await _context.Faculties
                .Where(f => f.AccountId == accountId && !f.IsDeleted)
                .Select(f => f.Id)
                .FirstOrDefaultAsync();

            if (facultyId == Guid.Empty)
                return new FacultyMetricDto { UnreadNotifications = 0, UpcomingClass = "N/A" };

            // -----------------------------
            // Unread notifications
            // -----------------------------
            var unreadNotifications = await _context.Notifications
                .Where(n => !n.IsDeleted && n.AccountId == accountId)
                .Where(n => !_context.NotificationReads
                    .Any(nr => nr.NotificationId == n.Id &&
                               nr.AccountId == accountId &&
                               nr.IsRead))
                .CountAsync();

            // -----------------------------
            // Current time
            // -----------------------------
            var now = DateTime.Now;
            var currentTime = TimeOnly.FromDateTime(now);
            var todayIndex = (int)now.DayOfWeek; // 0 = Sunday

            // -----------------------------
            // Faculty offerings with course code
            // -----------------------------
            var offerings = await (
                from co in _context.CourseOfferings
                join c in _context.Courses on co.CourseId equals c.Id
                where co.FacultyId == facultyId && !co.IsDeleted
                select new
                {
                    c.Code,
                    co.StartTime,
                    co.EndTime,
                    co.Mon,
                    co.Tue,
                    co.Wed,
                    co.Thu,
                    co.Fri,
                    co.Sat,
                    co.Sun
                }).ToListAsync();

            // -----------------------------
            // Helper: check if offering occurs on a DayOfWeek
            // -----------------------------
            bool IsOnDay(dynamic co, DayOfWeek day) => day switch
            {
                DayOfWeek.Monday => co.Mon,
                DayOfWeek.Tuesday => co.Tue,
                DayOfWeek.Wednesday => co.Wed,
                DayOfWeek.Thursday => co.Thu,
                DayOfWeek.Friday => co.Fri,
                DayOfWeek.Saturday => co.Sat,
                DayOfWeek.Sunday => co.Sun,
                _ => false
            };

            // -----------------------------
            // Find next upcoming class in the week
            // -----------------------------
            dynamic? nextClass = null;
            DayOfWeek? nextClassDay = null;
            int daysChecked = 0;

            while (daysChecked < 7 && nextClass == null)
            {
                var day = (DayOfWeek)((todayIndex + daysChecked) % 7);

                var classesOnDay = offerings
                    .Where(co => IsOnDay(co, day))
                    .OrderBy(co => co.StartTime)
                    .ToList();

                if (classesOnDay.Any())
                {
                    if (daysChecked == 0)
                    {
                        // Today: pick class that hasn't started yet
                        nextClass = classesOnDay.FirstOrDefault(co => co.StartTime >= currentTime);
                    }

                    // If no class left today or checking future day, pick first class
                    if (nextClass == null && daysChecked > 0)
                    {
                        nextClass = classesOnDay.First();
                    }

                    if (nextClass != null)
                    {
                        nextClassDay = day; // store the day of next class
                        break;
                    }
                }

                daysChecked++;
            }

            // -----------------------------
            // Format next class string
            // -----------------------------
            string upcomingClassStr = (nextClass != null && nextClassDay.HasValue)
                ? $"{nextClassDay.Value.ToString().Substring(0, 3)} {nextClass.StartTime:hh\\:mm tt} - {nextClass.Code}"
                : "N/A";

            // -----------------------------
            // Return dashboard DTO
            // -----------------------------
            return new FacultyMetricDto
            {
                UnreadNotifications = unreadNotifications,
                UpcomingClass = upcomingClassStr
            };
        }

    }
}
