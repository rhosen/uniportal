using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;
using UniPortal.Dtos.Attendance;

namespace UniPortal.Services.Portals
{
    public class AttendanceService
    {
        private readonly UniPortalContext _db;

        public AttendanceService(UniPortalContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Get attendance records for a single course and date.
        /// Handles canceled classes by overriding status to Absent and remarks to "Class canceled".
        /// </summary>
        public async Task<AttendanceViewDto> GetAttendanceForDateAsync(Guid courseOfferingId, DateTime date)
        {
            // Check if class is canceled
            var cancellation = await _db.ClassCancellations
                .Where(c => c.CourseOfferingId == courseOfferingId && c.CancellationDate.Date == date.Date)
                .FirstOrDefaultAsync();

            // Fetch students enrolled in this course (using joins, no navigation properties)
            var students = await (
                from s in _db.Students
                join e in _db.Enrollments on s.Id equals e.StudentId
                join a in _db.Accounts on s.AccountId equals a.Id
                where e.CourseOfferingId == courseOfferingId
                orderby a.FirstName
                select new AttendanceRowDto
                {
                    StudentId = s.Id,
                    StudentName = a.FirstName + " " + a.LastName,
                    Status = "Absent",
                    Remarks = ""
                }
            ).ToListAsync();

            // Fetch existing attendance for this date
            var existing = await _db.Attendances
                .Where(a => a.CourseOfferingId == courseOfferingId && a.AttendanceDate.Date == date.Date)
                .ToListAsync();

            foreach (var row in students)
            {
                var att = existing.FirstOrDefault(a => a.StudentId == row.StudentId);
                if (att != null)
                {
                    row.Status = att.Status;
                    row.Remarks = att.Remarks ?? "";
                }

                // If class canceled, override
                if (cancellation != null)
                {
                    row.Status = "Absent";
                    row.Remarks = "Class canceled";
                }
            }

            return new AttendanceViewDto
            {
                CourseOfferingId = courseOfferingId,
                Date = date,
                IsCanceled = cancellation != null,
                CancellationReason = cancellation?.Reason,
                Students = students
            };
        }

        /// <summary>
        /// Add or update attendance records for a given course and date.
        /// Throws exception if trying to update past dates.
        /// Does not allow saving for canceled classes.
        /// </summary>
        public async Task SaveAttendanceAsync(Guid courseOfferingId, DateTime date, List<AttendanceRowDto> rows)
        {
            // Validate date is not in the past
            if (date.Date < DateTime.Today)
                throw new InvalidOperationException("Cannot update attendance for past dates.");

            // Check if class is canceled
            var canceled = await _db.ClassCancellations
                .AnyAsync(c => c.CourseOfferingId == courseOfferingId && c.CancellationDate.Date == date.Date);
            if (canceled)
                throw new InvalidOperationException("Cannot update attendance for a canceled class.");

            foreach (var row in rows)
            {
                var att = await _db.Attendances
                    .FirstOrDefaultAsync(a => a.CourseOfferingId == courseOfferingId &&
                                              a.StudentId == row.StudentId &&
                                              a.AttendanceDate.Date == date.Date);

                if (att != null)
                {
                    // Edit existing
                    att.Status = row.Status;
                    att.Remarks = row.Remarks;
                }
                else
                {
                    // Add new
                    _db.Attendances.Add(new Attendance
                    {
                        StudentId = row.StudentId,
                        CourseOfferingId = courseOfferingId,
                        AttendanceDate = date,
                        Status = row.Status,
                        Remarks = row.Remarks
                    });
                }
            }

            await _db.SaveChangesAsync();
        }


        public async Task<List<SelectOption>> GetCourseOptionsAsync(Guid facultyId, Guid semesterId, Guid batchId, Guid sectionId)
        {
            return await (
                from co in _db.CourseOfferings
                join c in _db.Courses on co.CourseId equals c.Id
                where !c.IsDeleted
                      && co.SemesterId == semesterId
                      && co.BatchId == batchId
                      && co.SectionId == sectionId
                      && co.FacultyId == facultyId
                orderby c.Title
                select new SelectOption
                {
                    Id = co.Id,
                    Name = c.Title
                }
            ).ToListAsync();
        }

    }
}
