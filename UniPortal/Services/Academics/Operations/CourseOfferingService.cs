using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;

namespace UniPortal.Services.Academics.Operations
{
    public class CourseOfferingService
    {
        private readonly UniPortalContext _context;

        public CourseOfferingService(UniPortalContext context)
        {
            _context = context;
        }

        // 1️⃣ Load curriculum courses (for adding) if no offerings exist for batch + section
        public async Task<List<CourseOfferingDto>> GetCurriculumCoursesAsync(
            Guid programId, int semesterNumber, Guid batchId, Guid sectionId)
        {
            bool hasOfferings = await _context.CourseOfferings
                .AnyAsync(o => o.BatchId == batchId && o.SectionId == sectionId && !o.IsDeleted);

            if (hasOfferings)
                return new List<CourseOfferingDto>();

            var query =
                from c in _context.Curriculums
                join crs in _context.Courses on c.CourseId equals crs.Id
                join ct in _context.CourseTypes on c.CourseTypeId equals ct.Id
                where c.ProgramId == programId
                      && c.SemesterNumber == semesterNumber
                      && !c.IsDeleted
                orderby c.SequenceOrder
                select new CourseOfferingDto
                {
                    CurriculumId = c.Id,
                    CourseId = crs.Id,
                    CourseTitle = crs.Title,
                    CourseType = ct.Name,
                    CourseTypeId = ct.Id,
                    SequenceOrder = c.SequenceOrder,
                    FacultyId = Guid.Empty,
                    FacultyName = string.Empty,
                    StartTime = null,
                    EndTime = null,
                    MaxEnrollment = 0,
                    Mon = false,
                    Tue = false,
                    Wed = false,
                    Thu = false,
                    Fri = false,
                    Sat = false,
                    Sun = false,
                    SemesterNumber = c.SemesterNumber,
                    BatchId = batchId,
                    SectionId = sectionId
                };

            return await query.ToListAsync();
        }

        // 2️⃣ Load existing offerings (for editing)
        public async Task<List<CourseOfferingDto>> GetOfferingsAsync(
            Guid programId, int semesterNumber, Guid batchId, Guid sectionId)
        {
            var query =
                from o in _context.CourseOfferings
                join crs in _context.Courses on o.CourseId equals crs.Id
                join ct in _context.CourseTypes on o.CourseTypeId equals ct.Id
                join f in _context.Faculties on o.FacultyId equals f.Id into fJoin
                from f in fJoin.DefaultIfEmpty()
                join a in _context.Accounts on f.AccountId equals a.Id into aJoin
                from a in aJoin.DefaultIfEmpty()
                where o.ProgramId == programId
                      && o.BatchId == batchId
                      && o.SectionId == sectionId
                      && !o.IsDeleted
                orderby o.SequenceOrder
                select new CourseOfferingDto
                {
                    Id = o.Id,
                    CurriculumId = o.CurriculumId,
                    CourseId = o.CourseId,
                    CourseTitle = crs.Title,
                    CourseType = ct.Name,
                    CourseTypeId = ct.Id,
                    SequenceOrder = o.SequenceOrder,
                    FacultyId = f != null ? f.Id : Guid.Empty,
                    FacultyName = a != null && (a.FirstName + " " + a.LastName).Trim() != string.Empty
                        ? (a.FirstName + " " + a.LastName).Trim()
                        : (f != null ? f.FacultyNumber : string.Empty),
                    StartTime = o.StartTime,
                    EndTime = o.EndTime,
                    MaxEnrollment = o.MaxEnrollment,
                    Mon = o.Mon,
                    Tue = o.Tue,
                    Wed = o.Wed,
                    Thu = o.Thu,
                    Fri = o.Fri,
                    Sat = o.Sat,
                    Sun = o.Sun,
                    SemesterNumber = semesterNumber, // snapshot from input
                    BatchId = o.BatchId,
                    SectionId = o.SectionId
                };

            return await query.ToListAsync();
        }

        // 3️⃣ Add or update offerings (partial save allowed)
        public async Task AddOrUpdateOfferingsAsync(
            Guid programId, int semesterNumber, Guid batchId, Guid sectionId, List<CourseOfferingDto> dtos)
        {
            var existingOfferings = await _context.CourseOfferings
                .Where(o => o.ProgramId == programId
                            && o.BatchId == batchId
                            && o.SectionId == sectionId
                            && !o.IsDeleted)
                .ToListAsync();

            foreach (var dto in dtos)
            {
                var offering = existingOfferings.FirstOrDefault(o => o.CourseId == dto.CourseId);

                if (offering == null)
                {
                    // Add new
                    offering = new CourseOffering
                    {
                        Id = Guid.NewGuid(),
                        CurriculumId = dto.CurriculumId,
                        CourseId = dto.CourseId,
                        SemesterId = Guid.Empty, // link to semester if needed
                        ProgramId = programId,
                        BatchId = batchId,
                        SectionId = sectionId,
                        FacultyId = dto.FacultyId,
                        SequenceOrder = dto.SequenceOrder,
                        CreditHours = dto.CreditHours,
                        CourseTypeId = dto.CourseTypeId,
                        MaxEnrollment = dto.MaxEnrollment,
                        Mon = dto.Mon,
                        Tue = dto.Tue,
                        Wed = dto.Wed,
                        Thu = dto.Thu,
                        Fri = dto.Fri,
                        Sat = dto.Sat,
                        Sun = dto.Sun,
                        StartTime = dto.StartTime,
                        EndTime = dto.EndTime,
                        IsDeleted = false,
                        CurrentEnrollment = 0,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.CourseOfferings.Add(offering);
                }
                else
                {
                    // Update existing
                    offering.FacultyId = dto.FacultyId;
                    offering.SequenceOrder = dto.SequenceOrder;
                    offering.StartTime = dto.StartTime;
                    offering.EndTime = dto.EndTime;
                    offering.MaxEnrollment = dto.MaxEnrollment;
                    offering.Mon = dto.Mon;
                    offering.Tue = dto.Tue;
                    offering.Wed = dto.Wed;
                    offering.Thu = dto.Thu;
                    offering.Fri = dto.Fri;
                    offering.Sat = dto.Sat;
                    offering.Sun = dto.Sun;
                    offering.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
        }

        // 4️⃣ Soft delete offering
        public async Task DeleteAsync(Guid offeringId)
        {
            var offering = await _context.CourseOfferings
                .FirstOrDefaultAsync(o => o.Id == offeringId && !o.IsDeleted);

            if (offering == null)
                throw new InvalidOperationException("Course offering not found or already deleted.");

            // Check related entities without navigation
            bool hasEnrollments = await _context.Enrollments.AnyAsync(e => e.CourseOfferingId == offeringId && !e.IsDeleted);
            bool hasAssignments = await _context.Assignments.AnyAsync(a => a.CourseOfferingId == offeringId && !a.IsDeleted);

            if (hasEnrollments || hasAssignments)
                throw new InvalidOperationException("Cannot delete this course offering because students are enrolled or assignments exist.");

            offering.IsDeleted = true;
            offering.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }


        public async Task<List<CourseDto>> GetEligibleCoursesForStudentAsync(Guid studentId, int semesterNumber)
        {
            // Fetch student's program, batch, section
            var student = await _context.Students
                .Where(s => s.Id == studentId && !s.IsDeleted)
                .Select(s => new { s.ProgramId, s.BatchId, s.SectionId })
                .FirstOrDefaultAsync();

            if (student == null) return new List<CourseDto>();

            // Get eligible course offerings
            var query = from co in _context.CourseOfferings
                        join c in _context.Courses on co.CourseId equals c.Id
                        join t in _context.Accounts on co.FacultyId equals t.Id
                        where co.ProgramId == student.ProgramId
                              && co.BatchId == student.BatchId
                              && co.SectionId == student.SectionId
                              && co.SemesterNumber == semesterNumber
                              && !co.IsDeleted
                        select new CourseDto
                        {
                            Id = co.Id,
                            CourseName = c.Code + " - " + c.Title,
                            TeacherName = t.FirstName + " " + t.LastName,
                            Credits = co.CreditHours
                        };

            return await query.ToListAsync();
        }

    }
}
