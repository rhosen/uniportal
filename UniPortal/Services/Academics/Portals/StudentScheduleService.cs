using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Dtos.Schedule;

namespace UniPortal.Services.Academics.Portals
{
    public class StudentScheduleService
    {
        private readonly UniPortalContext _dbContext;

        public StudentScheduleService(UniPortalContext db)
        {
            _dbContext = db;
        }

        public List<StudentScheduleCourseDto> GetStudentCourses(Guid studentId, Guid semesterId, List<DateTime> weekDates)
        {
            // Step 1: Join tables (no navigation properties)
            var query = from e in _dbContext.Enrollments
                        join co in _dbContext.CourseOfferings on e.CourseOfferingId equals co.Id
                        join c in _dbContext.Courses on co.CourseId equals c.Id
                        join f in _dbContext.Faculties on co.FacultyId equals f.Id
                        join a in _dbContext.Accounts on f.AccountId equals a.Id
                        join r in _dbContext.Rooms on co.RoomId equals r.Id
                        where e.StudentId == studentId
                              && co.SemesterId == semesterId
                              && !e.IsDeleted
                        select new { co, c, a, r };

            var courses = query.AsEnumerable().ToList();
            var courseIds = courses.Select(x => x.co.Id).ToList();

            // Step 2: Get cancellations
            var cancellations = _dbContext.ClassCancellations
                .Where(cc => courseIds.Contains(cc.CourseOfferingId))
                .ToList();

            // Step 3: Flatten per-day sessions
            var result = new List<StudentScheduleCourseDto>();

            foreach (var course in courses)
            {
                foreach (var date in weekDates)
                {
                    var dayOfWeek = date.DayOfWeek;
                    bool meetsToday = dayOfWeek switch
                    {
                        DayOfWeek.Monday => course.co.Mon,
                        DayOfWeek.Tuesday => course.co.Tue,
                        DayOfWeek.Wednesday => course.co.Wed,
                        DayOfWeek.Thursday => course.co.Thu,
                        DayOfWeek.Friday => course.co.Fri,
                        DayOfWeek.Saturday => course.co.Sat,
                        DayOfWeek.Sunday => course.co.Sun,
                        _ => false
                    };

                    if (!meetsToday) continue;

                    var cancelled = cancellations.FirstOrDefault(cc =>
                        cc.CourseOfferingId == course.co.Id &&
                        cc.CancellationDate.Date == date.Date
                    );

                    result.Add(new StudentScheduleCourseDto
                    {
                        CourseOfferingId = course.co.Id,
                        Code = course.c.Code,
                        Title = course.c.Title,
                        Faculty = course.a.FirstName + " " + course.a.LastName,
                        Room = course.r.RoomName,
                        Credits = course.c.CreditHours,
                        StartTime = course.co.StartTime.ToTimeSpan(),
                        EndTime = course.co.EndTime.ToTimeSpan(),
                        Date = date,
                        IsCancelled = cancelled != null,
                        CancellationReason = cancelled?.Reason
                    });
                }
            }

            return result;
        }
    }
}
