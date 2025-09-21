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
            var query = from f in _context.Faculties
                        join a in _context.Accounts on f.AccountId equals a.Id
                        where f.AccountId == accountId && !f.IsDeleted && !a.IsDeleted
                        select new FacultyProfileViewModel
                        {
                            Id = a.Id,
                            FullName = a.FirstName + " " + a.LastName,
                            Email = a.Email,
                            Phone = a.Phone
                        };

            return await query.FirstOrDefaultAsync();
        }

        // -----------------------------
        // Get dashboard metrics
        // -----------------------------
        public async Task<FacultyMetricsViewModel> GetDashboardMetricsAsync(Guid accountId)
        {
            // Get faculty ID
            var facultyId = await _context.Faculties
                .Where(f => f.AccountId == accountId && !f.IsDeleted)
                .Select(f => f.Id)
                .FirstOrDefaultAsync();

            if (facultyId == Guid.Empty)
                return new FacultyMetricsViewModel { TotalCourses = 0, UpcomingClass = "N/A" };

            // Total courses taught
            int totalCourses = await _context.CourseOfferings
                .Where(co => co.FacultyId == facultyId && !co.IsDeleted)
                .CountAsync();

            // Current day and time
            var now = DateTime.Now;
            var currentDayOfWeek = now.DayOfWeek;
            var currentTime = TimeOnly.FromDateTime(now);

            // Next class calculation using CourseOfferings + explicit join to Courses
            var offeringsQuery = from co in _context.CourseOfferings
                                 join c in _context.Courses on co.CourseId equals c.Id
                                 where co.FacultyId == facultyId && !co.IsDeleted
                                 select new
                                 {
                                     c.Title,
                                     co.StartTime,
                                     co.EndTime,
                                     co.Mon,
                                     co.Tue,
                                     co.Wed,
                                     co.Thu,
                                     co.Fri,
                                     co.Sat,
                                     co.Sun
                                 };

            var offerings = await offeringsQuery.ToListAsync();

            var weekdayFlagMap = new Dictionary<DayOfWeek, Func<dynamic, bool>>
            {
                { DayOfWeek.Monday, x => x.Mon },
                { DayOfWeek.Tuesday, x => x.Tue },
                { DayOfWeek.Wednesday, x => x.Wed },
                { DayOfWeek.Thursday, x => x.Thu },
                { DayOfWeek.Friday, x => x.Fri },
                { DayOfWeek.Saturday, x => x.Sat },
                { DayOfWeek.Sunday, x => x.Sun }
            };

            var nextClassEntry = offerings
                .Where(co => weekdayFlagMap[currentDayOfWeek](co) && co.StartTime >= currentTime)
                .OrderBy(co => co.StartTime)
                .FirstOrDefault();

            string nextClass = nextClassEntry != null
                ? $"{nextClassEntry.StartTime:hh\\:mm} - {nextClassEntry.Title}"
                : "N/A";

            return new FacultyMetricsViewModel
            {
                TotalCourses = totalCourses,
                UpcomingClass = nextClass
            };
        }

    }
}
