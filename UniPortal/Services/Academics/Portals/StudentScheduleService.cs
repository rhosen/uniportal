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

        public List<StudentScheduleCourseDto> GetStudentCourses(Guid studentId, Guid semesterId)
        {
            // Fetch minimal data first
            var query = (from e in _dbContext.Enrollments
                         join co in _dbContext.CourseOfferings on e.CourseOfferingId equals co.Id
                         join c in _dbContext.Courses on co.CourseId equals c.Id
                         join f in _dbContext.Faculties on co.FacultyId equals f.Id
                         join a in _dbContext.Accounts on f.AccountId equals a.Id
                         join r in _dbContext.Rooms on co.RoomId equals r.Id
                         join cancel in _dbContext.ClassCancellations
                             .Where(cc => cc.CancellationDate.Date >= DateTime.Today.AddDays(-7)) // optional filter
                             on co.Id equals cancel.CourseOfferingId into cancels
                         from cancel in cancels.DefaultIfEmpty()
                         where e.StudentId == studentId && co.SemesterId == semesterId && !e.IsDeleted
                         select new
                         {
                             c.Code,
                             c.Title,
                             Faculty = a.FirstName + " " + a.LastName,
                             co.Mon,
                             co.Tue,
                             co.Wed,
                             co.Thu,
                             co.Fri,
                             co.Sat,
                             co.Sun,
                             co.StartTime,
                             co.EndTime,
                             r.RoomName,
                             c.CreditHours,
                             Cancelled = cancel != null,
                             CancelReason = cancel != null ? cancel.Reason : null,
                             CancelDate = cancel != null ? cancel.CancellationDate : (DateTime?)null
                         })
                        .AsEnumerable()
                        .Select(x => new StudentScheduleCourseDto
                        {
                            Code = x.Code,
                            Title = x.Title,
                            Faculty = x.Faculty,
                            Days = string.Join(", ", new[]
                            {
                        x.Mon ? "Mon" : null,
                        x.Tue ? "Tue" : null,
                        x.Wed ? "Wed" : null,
                        x.Thu ? "Thu" : null,
                        x.Fri ? "Fri" : null,
                        x.Sat ? "Sat" : null,
                        x.Sun ? "Sun" : null
                            }.Where(d => d != null)),
                            Time = $"{x.StartTime:hh\\:mm}–{x.EndTime:hh\\:mm}",
                            Room = x.RoomName,
                            Credits = x.CreditHours,
                            StartTime = x.StartTime.ToTimeSpan(),
                            EndTime = x.EndTime.ToTimeSpan(),
                            IsCancelled = x.Cancelled,
                            CancellationReason = x.CancelReason,
                            CancellationDate = x.CancelDate
                        })
                        .ToList();

            return query;
        }

    }
}
