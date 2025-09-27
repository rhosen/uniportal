using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;
using UniPortal.Dtos.Attendance;

namespace UniPortal.Services.Portals
{
    public class AttendanceReportService
    {
        private readonly UniPortalContext _db;

        public AttendanceReportService(UniPortalContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Get all attendance records for a student for a specific course.
        /// Includes canceled classes.
        /// </summary>
        public async Task<List<AttendanceReportDto>> GetAttendanceForStudentCourseAsync(Guid studentId, Guid courseOfferingId)
        {
            var student = await _db.Students
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return new List<AttendanceReportDto>();

            var attendanceRecords = await _db.Attendances
                .Where(a => a.CourseOfferingId == courseOfferingId && a.StudentId == student.Id)
                .ToListAsync();

            var canceledDates = await _db.ClassCancellations
                .Where(c => c.CourseOfferingId == courseOfferingId)
                .ToListAsync();

            var allDates = attendanceRecords.Select(a => a.AttendanceDate)
                .Union(canceledDates.Select(c => c.CancellationDate))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            var result = new List<AttendanceReportDto>();

            foreach (var date in allDates)
            {
                var att = attendanceRecords.FirstOrDefault(a => a.AttendanceDate.Date == date.Date);
                var canceled = canceledDates.FirstOrDefault(c => c.CancellationDate.Date == date.Date);

                var dto = new AttendanceReportDto
                {
                    Date = date,
                    Status = att?.Status ?? "Absent",
                    Remarks = att?.Remarks ?? "",
                    IsCanceled = canceled != null,
                    CancellationReason = canceled?.Reason
                };

                if (canceled != null)
                {
                    dto.Status = "Absent";
                    dto.Remarks = canceled.Reason ?? "Class canceled";
                }

                result.Add(dto);
            }

            return result;
        }

        /// <summary>
        /// Get all courses for a student in a specific semester.
        /// </summary>
        public async Task<List<SelectOption>> GetCourseOptionsForStudentAsync(Guid studentId, Guid semesterId)
        {
            return await (
                from e in _db.Enrollments
                join co in _db.CourseOfferings on e.CourseOfferingId equals co.Id
                join c in _db.Courses on co.CourseId equals c.Id
                where !c.IsDeleted
                      && co.SemesterId == semesterId
                      && e.StudentId == studentId
                orderby c.Title
                select new SelectOption
                {
                    Id = co.Id,
                    Name = c.Title
                }
            ).Distinct().ToListAsync();
        }
    }
}
