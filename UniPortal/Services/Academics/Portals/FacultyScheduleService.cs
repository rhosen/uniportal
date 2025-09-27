using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos.Schedule;

public class FacultyScheduleService
{
    private readonly UniPortalContext _dbContext;

    public FacultyScheduleService(UniPortalContext db)
    {
        _dbContext = db;
    }

    public List<FacultyScheduleCourseDto> GetFacultyCourses(Guid facultyId, Guid semesterId, List<DateTime> weekDates)
    {
        var query = from co in _dbContext.CourseOfferings
                    join c in _dbContext.Courses on co.CourseId equals c.Id
                    join r in _dbContext.Rooms on co.RoomId equals r.Id
                    join b in _dbContext.Batches on co.BatchId equals b.Id
                    join s in _dbContext.Sections on co.SectionId equals s.Id
                    where co.FacultyId == facultyId && co.SemesterId == semesterId
                    select new { co, c, r, b, s };

        var courses = query.AsEnumerable().ToList();
        var courseIds = courses.Select(x => x.co.Id).ToList();

        var cancellations = _dbContext.ClassCancellations
            .Where(cc => courseIds.Contains(cc.CourseOfferingId))
            .ToList();

        var result = new List<FacultyScheduleCourseDto>();

        foreach (var course in courses)
        {
            foreach (var date in weekDates)
            {
                // Check if course meets on this day
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

                // Check if canceled on this date
                var cancelled = cancellations.FirstOrDefault(cc =>
                    cc.CourseOfferingId == course.co.Id &&
                    cc.CancellationDate.Date == date.Date
                );

                result.Add(new FacultyScheduleCourseDto
                {
                    CourseOfferingId = course.co.Id,
                    Code = course.c.Code,
                    Title = course.c.Title,
                    Batch = course.b.Name,
                    Section = course.s.Name,
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

    public async Task CancelClassAsync(Guid facultyId, Guid courseOfferingId, DateTime cancellationDate, string? reason)
    {
        // Check if course exists and belongs to this faculty
        var course = await _dbContext.CourseOfferings
            .FirstOrDefaultAsync(c => c.Id == courseOfferingId && c.FacultyId == facultyId);

        if (course == null)
            throw new InvalidOperationException("Course not found or does not belong to this faculty.");

        // Prevent cancellation of past classes
        if (cancellationDate.Date < DateTime.Today)
            throw new InvalidOperationException("Cannot cancel a class that has already been conducted.");

        // Prevent duplicate cancellation
        var existing = await _dbContext.ClassCancellations
            .FirstOrDefaultAsync(c => c.CourseOfferingId == courseOfferingId && c.CancellationDate.Date == cancellationDate.Date);

        if (existing != null)
            throw new InvalidOperationException("This class session has already been cancelled.");

        // Add cancellation record
        _dbContext.ClassCancellations.Add(new ClassCancellation
        {
            CourseOfferingId = courseOfferingId,
            CancellationDate = cancellationDate,
            Reason = reason
        });

        await _dbContext.SaveChangesAsync();
    }

}
