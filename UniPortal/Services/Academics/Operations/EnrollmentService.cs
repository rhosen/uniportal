using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;
using UniPortal.Services.Accounts;

public class EnrollmentService
{
    private readonly UniPortalContext _context;
    private readonly StudentService _studentService;

    public EnrollmentService(UniPortalContext context,
        StudentService studentService)
    {
        _context = context;
        _studentService = studentService;
    }

    // -----------------------------
    // Get eligible courses with enrollment status
    // -----------------------------
    public async Task<List<EnrollmentCourseDto>> GetEligibleWithEnrollmentStatusAsync(Guid studentId, int semesterNumber)
    {
        var student = await _context.Students
            .Where(s => s.Id == studentId && !s.IsDeleted)
            .Select(s => new { s.ProgramId, s.BatchId, s.SectionId })
            .FirstOrDefaultAsync();

        if (student == null) return new List<EnrollmentCourseDto>();

        var query = from co in _context.CourseOfferings
                    join c in _context.Courses on co.CourseId equals c.Id
                    join f in _context.Faculties on co.FacultyId equals f.Id
                    join a in _context.Accounts on f.AccountId equals a.Id
                    let isEnrolled = _context.Enrollments
                        .Any(e => e.StudentId == studentId && e.CourseOfferingId == co.Id && !e.IsDeleted)
                    where co.ProgramId == student.ProgramId
                          && co.BatchId == student.BatchId
                          && co.SectionId == student.SectionId
                          && co.SemesterNumber == semesterNumber
                          && !co.IsDeleted
                    select new EnrollmentCourseDto
                    {
                        Id = co.Id,
                        CourseTitle = c.Code + " - " + c.Title,
                        FacultyName = a.FirstName + " " + a.LastName,
                        CreditHours = co.CreditHours,
                        MaxEnrollment = co.MaxEnrollment,
                        CurrentEnrollment = co.CurrentEnrollment,
                        IsEnrolled = isEnrolled,
                        Schedule = $"{(co.Mon ? "Mon, " : "")}" +
                                   $"{(co.Tue ? "Tue, " : "")}" +
                                   $"{(co.Wed ? "Wed, " : "")}" +
                                   $"{(co.Thu ? "Thu, " : "")}" +
                                   $"{(co.Fri ? "Fri, " : "")}" +
                                   $"{(co.Sat ? "Sat, " : "")}" +
                                   $"{(co.Sun ? "Sun, " : "")}".TrimEnd(',', ' ') +
                                   $" {co.StartTime:hh\\:mm} - {co.EndTime:hh\\:mm}"
                    };

        return await query.ToListAsync();
    }

    // -----------------------------
    // Get past enrollments grouped by semester
    // -----------------------------
    public async Task<Dictionary<string, List<EnrollmentCourseDto>>> GetPastEnrollmentsGroupedBySemesterAsync(
        Guid studentId, int currentSemesterNumber)
    {
        var query = from e in _context.Enrollments
                    join co in _context.CourseOfferings on e.CourseOfferingId equals co.Id
                    join c in _context.Courses on co.CourseId equals c.Id
                    join t in _context.Accounts on co.FacultyId equals t.Id
                    join sem in _context.Semesters on co.SemesterId equals sem.Id
                    where e.StudentId == studentId
                          && co.SemesterNumber < currentSemesterNumber
                          && !e.IsDeleted
                    orderby co.SemesterNumber
                    select new
                    {
                        SemesterName = $"{sem.SemesterType} ({sem.StartDate:MMM yyyy} - {sem.EndDate:MMM yyyy}) - Semester {co.SemesterNumber}",
                        Course = new EnrollmentCourseDto
                        {
                            Id = co.Id,
                            CourseTitle = c.Code + " - " + c.Title,
                            FacultyName = t.FirstName + " " + t.LastName,
                            CreditHours = co.CreditHours
                        }
                    };

        var list = await query.ToListAsync();

        return list
            .GroupBy(x => x.SemesterName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.Course).ToList()
            );
    }

    private string GetCourseTitle(Guid courseOfferingId)
    {
        var title = (from co in _context.CourseOfferings
                     join c in _context.Courses on co.CourseId equals c.Id
                     where co.Id == courseOfferingId
                     select c.Title)
                     .FirstOrDefault();

        return title ?? courseOfferingId.ToString();
    }

    private async Task<List<Enrollment>> GetExistingEnrollmentsAsync(Guid studentId)
    {
        var query = from e in _context.Enrollments
                    join co in _context.CourseOfferings on e.CourseOfferingId equals co.Id
                    join s in _context.Students on e.StudentId equals s.Id
                    where e.StudentId == studentId
                          && !e.IsDeleted
                          && !co.IsDeleted
                          && co.SemesterNumber == s.CurrentSemester
                    select e;

        return await query.ToListAsync();
    }

    private async Task<HashSet<Guid>> GetGradedCourseIdsAsync(Guid studentId)
    {
        var ids = await _context.Grades
            .Where(g => g.StudentId == studentId)
            .Select(g => g.CourseOfferingId)
            .ToListAsync();

        return ids.ToHashSet();
    }

    public async Task UpdateEnrollmentsAsync(Guid studentId, List<Guid> offeredCourseIds, Guid modifiedBy)
    {
        // 1. Load core data
        int semesterNumber = await _studentService.GetCurrentSemesterNumber(studentId);
        var existingEnrollments = await GetExistingEnrollmentsAsync(studentId);
        var existingIds = existingEnrollments.Select(e => e.CourseOfferingId).ToHashSet();
        var gradedIds = await GetGradedCourseIdsAsync(studentId);

        var offerings = await _context.CourseOfferings
            .Where(co => co.SemesterNumber == semesterNumber && !co.IsDeleted)
            .ToListAsync();

        var offeredSet = offeredCourseIds.ToHashSet();

        // 2. Validate all actions before making changes
        ValidateEnrollments(offeredSet, existingIds, gradedIds, offerings);

        // 3. Remove unchecked enrollments
        RemoveUncheckedEnrollments(existingEnrollments, offeredSet, gradedIds, offerings, modifiedBy);

        // 4. Add new enrollments
        AddNewEnrollments(studentId, offeredSet, existingIds, offerings, modifiedBy);

        // 5. Save once
        await _context.SaveChangesAsync();
    }

    private void ValidateEnrollments(
    HashSet<Guid> offeredSet,
    HashSet<Guid> existingIds,
    HashSet<Guid> gradedIds,
    List<CourseOffering> offerings)
    {
        if (!offeredSet.Except(existingIds).Any() && offeredSet.SetEquals(existingIds))
            throw new InvalidOperationException("No enrollment changes detected.");

        foreach (var coId in offeredSet.Except(existingIds))
        {
            var co = offerings.FirstOrDefault(x => x.Id == coId)
                     ?? throw new InvalidOperationException($"CourseOffering {coId} not found.");

            if (co.CurrentEnrollment >= co.MaxEnrollment)
            {
                var title = GetCourseTitle(coId);
                throw new InvalidOperationException(
                    $"Cannot enroll in {title}: capacity reached.");
            }
        }

        foreach (var coId in existingIds.Except(offeredSet))
        {
            if (gradedIds.Contains(coId))
            {
                var title = GetCourseTitle(coId);
                throw new InvalidOperationException(
                    $"Cannot remove course {title}, grading already done.");
            }
        }
    }


    private void AddNewEnrollments(
        Guid studentId,
        HashSet<Guid> offeredSet,
        HashSet<Guid> existingIds,
        List<CourseOffering> offerings,
        Guid modifiedBy)
    {
        var toAddIds = offeredSet.Except(existingIds);

        foreach (var coId in toAddIds)
        {
            var co = offerings.First(x => x.Id == coId);

            _context.Enrollments.Add(new Enrollment
            {
                Id = Guid.NewGuid(),
                StudentId = studentId,
                CourseOfferingId = coId,
                ModifiedById = modifiedBy,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            });

            co.CurrentEnrollment++;
        }
    }

    private void RemoveUncheckedEnrollments(
        List<Enrollment> existingEnrollments,
        HashSet<Guid> offeredSet,
        HashSet<Guid> gradedIds,
        List<CourseOffering> offerings,
        Guid modifiedBy)
    {
        foreach (var enrollment in existingEnrollments)
        {
            if (offeredSet.Contains(enrollment.CourseOfferingId)) continue;

            enrollment.IsDeleted = true;
            enrollment.ModifiedById = modifiedBy;
            enrollment.UpdatedAt = DateTime.Now;

            var co = offerings.FirstOrDefault(x => x.Id == enrollment.CourseOfferingId);
            if (co != null && co.CurrentEnrollment > 0)
                co.CurrentEnrollment--;
        }
    }

}
