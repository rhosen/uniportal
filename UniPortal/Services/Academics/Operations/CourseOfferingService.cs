using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;
using UniPortal.Services.Academics.Configs;

namespace UniPortal.Services.Academics.Operations
{
    public class CourseOfferingService
    {
        private readonly UniPortalContext _context;
        private readonly CurriculumService _curriculumService;
        private readonly SemesterService _semesterService;

        public CourseOfferingService(
            UniPortalContext context,
            CurriculumService curriculumService,
            SemesterService semesterService)
        {
            _context = context;
            _curriculumService = curriculumService;
            _semesterService = semesterService;
        }


        public async Task<Guid> GetOfferingSemesterIdAsync(Guid offeringId)
        {
            var semesterId = await _context.CourseOfferings
                .Where(co => co.Id == offeringId && !co.IsDeleted)
                .Select(co => co.SemesterId)
                .FirstOrDefaultAsync();

            if (semesterId == Guid.Empty)
                throw new InvalidOperationException("Offering not found.");

            return semesterId;
        }


        // -----------------------------
        // Get curriculum + existing offerings for a semester
        // -----------------------------
        public async Task<List<CourseOfferingDto>> GetOfferingsForSemesterAsync(
            Guid programId, Guid batchId, Guid sectionId, int semesterNumber)
        {
            // 1️⃣ Curriculum courses
            var curriculumCourses = await (
                from c in _context.Curriculums
                join co in _context.Courses on c.CourseId equals co.Id
                where c.ProgramId == programId && c.SemesterNumber == semesterNumber
                select new CourseOfferingDto
                {
                    CurriculumId = c.Id,
                    CourseId = c.CourseId,
                    CourseTitle = co.Code + " - " + co.Title,
                    CreditHours = co.CreditHours,
                    SequenceOrder = c.SequenceOrder
                }).ToListAsync();

            // 2️⃣ Existing offerings
            var existingOfferings = await (
                from o in _context.CourseOfferings
                join co in _context.Courses on o.CourseId equals co.Id
                where o.ProgramId == programId
                      && o.BatchId == batchId
                      && o.SectionId == sectionId
                      && o.SemesterNumber == semesterNumber
                      && !o.IsDeleted
                select new CourseOfferingDto
                {
                    CurriculumId = o.CurriculumId,
                    CourseId = o.CourseId,
                    CourseTitle = co.Code + " - " + co.Title,
                    CreditHours = o.CreditHours,
                    SequenceOrder = o.SequenceOrder,
                    FacultyId = o.FacultyId,
                    RoomId = o.RoomId,
                    Mon = o.Mon,
                    Tue = o.Tue,
                    Wed = o.Wed,
                    Thu = o.Thu,
                    Fri = o.Fri,
                    Sat = o.Sat,
                    Sun = o.Sun,
                    StartTime = o.StartTime,
                    EndTime = o.EndTime,
                    IsOffered = true,
                    OfferingId = o.Id
                }).ToListAsync();

            // Merge existing data into curriculum courses
            foreach (var c in curriculumCourses)
            {
                var existing = existingOfferings.FirstOrDefault(e => e.CourseId == c.CourseId);
                if (existing != null)
                {
                    c.IsOffered = true;
                    c.OfferingId = existing.OfferingId;
                    c.FacultyId = existing.FacultyId;
                    c.RoomId = existing.RoomId;
                    c.Mon = existing.Mon;
                    c.Tue = existing.Tue;
                    c.Wed = existing.Wed;
                    c.Thu = existing.Thu;
                    c.Fri = existing.Fri;
                    c.Sat = existing.Sat;
                    c.Sun = existing.Sun;
                    c.StartTime = existing.StartTime;
                    c.EndTime = existing.EndTime;
                    c.SequenceOrder = existing.SequenceOrder;
                    c.CreditHours = existing.CreditHours;
                }
            }

            return curriculumCourses;
        }

        private async Task<List<CourseOffering>> GetExistingOfferingsAsync(
            Guid programId, Guid batchId, Guid sectionId, int semesterNumber)
        {
            return await _context.CourseOfferings
                .Where(o => o.ProgramId == programId &&
                            o.BatchId == batchId &&
                            o.SectionId == sectionId &&
                            o.SemesterNumber == semesterNumber &&
                            !o.IsDeleted)
                .ToListAsync();
        }


        // -----------------------------
        // Save offerings (add/update/delete) with validation and conflict checks
        // -----------------------------
        public async Task SaveOfferingsAsync(List<CourseOfferingDto> offerings,
                             Guid programId, Guid batchId, Guid sectionId, Guid selectedSemesterId, int semesterNumber)
        {
            if (offerings.Count == 0) return;

            await EnsureSemesterIsCurrentAsync(semesterNumber, selectedSemesterId);

            // 1️⃣ Validation + conflict checks
            await ValidateAllOfferingsAsync(offerings, selectedSemesterId, programId, batchId, sectionId, semesterNumber);

            // 2️⃣ Remove or delete offerings
            await RemoveOfferingsAsync(offerings, programId, batchId, sectionId, semesterNumber);

            // 3️⃣ Add or update offerings
            await AddOrUpdateOfferingsAsync(offerings, programId, batchId, sectionId, selectedSemesterId, semesterNumber);

            // 4️⃣ Commit changes
            await _context.SaveChangesAsync();
        }

        private async Task EnsureSemesterIsCurrentAsync(int semesterNumber, Guid semesterId)
        {
            var semester = await _semesterService.GetCurrentSemesterAsync();

            if (semester.Id != semesterId)
                throw new InvalidOperationException(
                    $"Cannot modify offerings for semester {semesterNumber} because it is not the current semester.");
        }

        // -----------------------------
        // Validation + conflict checks
        // -----------------------------
        private async Task ValidateAllOfferingsAsync(List<CourseOfferingDto> offerings, Guid semesterId,
             Guid programId, Guid batchId, Guid sectionId, int semesterNumber)
        {
            var existingOfferings = await GetExistingOfferingsAsync(programId, batchId, sectionId, semesterNumber);
            var offeredCourseCount = offerings.Count(x => x.IsOffered);

            if (offeredCourseCount == 0 && existingOfferings.Count == 0)
                throw new InvalidOperationException("No courses selected to save or remove.");

            // Build a HashSet for faster lookup
            var existingCourseIds = existingOfferings.Select(x => x.CourseId).ToHashSet();

            foreach (var course in offerings)
            {
                // Validate courses that are newly offered or were previously offered
                if (course.IsOffered || existingCourseIds.Contains(course.CourseId))
                {
                    ValidateOffering(course);
                    await CheckRoomConflictAsync(course, semesterId);
                    await CheckFacultyTimeConflictAsync(course, programId, batchId, sectionId, semesterNumber);
                    await CheckBatchTimeConflictAsync(course, programId, batchId, sectionId, semesterNumber);
                }
            }
        }

        private void ValidateOffering(CourseOfferingDto course)
        {
            if (course.RoomId == Guid.Empty)
                throw new InvalidOperationException($"Room must be selected for {course.CourseTitle}.");

            if (course.FacultyId == Guid.Empty)
                throw new InvalidOperationException($"Faculty must be selected for {course.CourseTitle}.");

            if (course.CurriculumId == Guid.Empty)
                throw new InvalidOperationException($"Curriculum must be selected for {course.CourseTitle}.");

            if (course.CourseId == Guid.Empty)
                throw new InvalidOperationException($"Course must be selected for {course.CourseTitle}.");

            if (course.StartTime == default)
                throw new InvalidOperationException($"Start time must be selected for {course.CourseTitle}.");

            if (course.EndTime == default)
                throw new InvalidOperationException($"End time must be selected for {course.CourseTitle}.");

            if (course.StartTime >= course.EndTime)
                throw new InvalidOperationException($"Start time must be earlier than end time for {course.CourseTitle}.");

            if (!(course.Mon || course.Tue || course.Wed || course.Thu || course.Fri || course.Sat || course.Sun))
                throw new InvalidOperationException($"At least one day must be selected for {course.CourseTitle}.");
        }


        // -----------------------------
        // Room / Faculty / Batch Conflict Checks
        // -----------------------------
        private async Task CheckRoomConflictAsync(CourseOfferingDto course, Guid semesterId)
        {
            var existingOfferings = _context.CourseOfferings
                .Where(x => x.RoomId == course.RoomId
                            && x.SemesterId == semesterId
                            && !x.IsDeleted);

            if (course.OfferingId.HasValue)
                existingOfferings = existingOfferings.Where(x => x.Id != course.OfferingId.Value);

            await foreach (var existing in existingOfferings.AsAsyncEnumerable())
            {
                bool timeOverlap = course.StartTime < existing.EndTime && course.EndTime > existing.StartTime;
                bool dayOverlap =
                    (course.Mon && existing.Mon) ||
                    (course.Tue && existing.Tue) ||
                    (course.Wed && existing.Wed) ||
                    (course.Thu && existing.Thu) ||
                    (course.Fri && existing.Fri) ||
                    (course.Sat && existing.Sat) ||
                    (course.Sun && existing.Sun);

                if (timeOverlap && dayOverlap)
                    throw new InvalidOperationException($"Room conflict detected for {course.CourseTitle}.");
            }
        }

        private async Task CheckFacultyTimeConflictAsync(CourseOfferingDto course, Guid programId, Guid batchId, Guid sectionId,
            int semesterNumber)
        {
            var existingOfferings = _context.CourseOfferings
                .Where(x => x.FacultyId == course.FacultyId
                            && x.ProgramId == programId
                            && x.BatchId == batchId
                            && x.SectionId == sectionId
                            && x.SemesterNumber == semesterNumber
                            && !x.IsDeleted);

            if (course.OfferingId.HasValue)
                existingOfferings = existingOfferings.Where(x => x.Id != course.OfferingId.Value);

            await foreach (var existing in existingOfferings.AsAsyncEnumerable())
            {
                bool timeOverlap = course.StartTime < existing.EndTime && course.EndTime > existing.StartTime;
                bool dayOverlap =
                    (course.Mon && existing.Mon) ||
                    (course.Tue && existing.Tue) ||
                    (course.Wed && existing.Wed) ||
                    (course.Thu && existing.Thu) ||
                    (course.Fri && existing.Fri) ||
                    (course.Sat && existing.Sat) ||
                    (course.Sun && existing.Sun);

                if (timeOverlap && dayOverlap)
                    throw new InvalidOperationException(
                        $"Schedule conflict detected for {course.CourseTitle} with faculty {course.FacultyName}.");
            }
        }

        private async Task CheckBatchTimeConflictAsync(
            CourseOfferingDto course, Guid programId, Guid batchId, Guid sectionId, int semesterNumber)
        {
            var existingOfferings = _context.CourseOfferings
                .Where(x => x.ProgramId == programId
                            && x.BatchId == batchId
                            && x.SectionId == sectionId
                            && x.SemesterNumber == semesterNumber
                            && !x.IsDeleted);

            if (course.OfferingId.HasValue)
                existingOfferings = existingOfferings.Where(x => x.Id != course.OfferingId.Value);

            await foreach (var existing in existingOfferings.AsAsyncEnumerable())
            {
                bool timeOverlap = course.StartTime < existing.EndTime && course.EndTime > existing.StartTime;
                bool dayOverlap =
                    (course.Mon && existing.Mon) ||
                    (course.Tue && existing.Tue) ||
                    (course.Wed && existing.Wed) ||
                    (course.Thu && existing.Thu) ||
                    (course.Fri && existing.Fri) ||
                    (course.Sat && existing.Sat) ||
                    (course.Sun && existing.Sun);

                if (timeOverlap && dayOverlap)
                    throw new InvalidOperationException(
                        $"Schedule conflict detected for {course.CourseTitle} with the same batch.");
            }
        }

        // -----------------------------
        // Add / Update Offerings
        // -----------------------------
        private async Task UpdateOfferingAsync(CourseOfferingDto course, Guid selectedSemesterId)
        {
            var existing = await _context.CourseOfferings.FindAsync(course.OfferingId.Value);
            if (existing != null)
            {
                existing.FacultyId = course.FacultyId;
                existing.RoomId = course.RoomId;
                existing.SemesterId = selectedSemesterId;
                existing.Mon = course.Mon;
                existing.Tue = course.Tue;
                existing.Wed = course.Wed;
                existing.Thu = course.Thu;
                existing.Fri = course.Fri;
                existing.Sat = course.Sat;
                existing.Sun = course.Sun;
                existing.StartTime = course.StartTime;
                existing.EndTime = course.EndTime;
                existing.SequenceOrder = course.SequenceOrder;
                existing.CreditHours = course.CreditHours;
            }
        }

        private async Task AddOfferingAsync(CourseOfferingDto course, Guid programId, Guid batchId, Guid sectionId, Guid selectedSemesterId, int semesterNumber)
        {
            var curriculum = await _context.Curriculums.FindAsync(course.CurriculumId);
            if (curriculum == null)
                throw new InvalidOperationException($"Curriculum not found for ID {course.CurriculumId}.");

            _context.CourseOfferings.Add(new CourseOffering
            {
                Id = Guid.NewGuid(),
                ProgramId = programId,
                BatchId = batchId,
                SectionId = sectionId,
                SemesterId = selectedSemesterId,
                SemesterNumber = semesterNumber,
                CurriculumId = curriculum.Id,
                CourseId = curriculum.CourseId,
                FacultyId = course.FacultyId,
                RoomId = course.RoomId,
                Mon = course.Mon,
                Tue = course.Tue,
                Wed = course.Wed,
                Thu = course.Thu,
                Fri = course.Fri,
                Sat = course.Sat,
                Sun = course.Sun,
                StartTime = course.StartTime,
                EndTime = course.EndTime,
                CreditHours = curriculum.CreditHours,
                SequenceOrder = curriculum.SequenceOrder,
                IsDeleted = false
            });
        }

        private async Task AddOrUpdateOfferingsAsync(List<CourseOfferingDto> offerings,
            Guid programId, Guid batchId, Guid sectionId, Guid selectedSemesterId, int semesterNumber)
        {
            foreach (var course in offerings)
            {
                if (course.OfferingId.HasValue)
                    await UpdateOfferingAsync(course, selectedSemesterId);
                else if (course.IsOffered)
                    await AddOfferingAsync(course, programId, batchId, sectionId, selectedSemesterId, semesterNumber);
            }
        }

        // -----------------------------
        // Remove or delete offerings
        // -----------------------------
        private async Task RemoveOfferingsAsync(
            List<CourseOfferingDto> offerings, Guid programId, Guid batchId, Guid sectionId, int semesterNumber)
        {
            var existingOfferings = await _context.CourseOfferings
                .Where(o =>
                    o.ProgramId == programId &&
                    o.BatchId == batchId &&
                    o.SectionId == sectionId &&
                    o.SemesterNumber == semesterNumber &&
                    !o.IsDeleted)
                .ToListAsync();

            var selectedCurriculumIds = offerings.Select(o => o.CurriculumId).ToHashSet();

            // Offerings that user intends to remove
            var offeringsToRemove = existingOfferings
                .Where(o => !selectedCurriculumIds.Contains(o.CurriculumId))
                .ToList();

            if (!offeringsToRemove.Any())
                return;

            // Only fetch course titles for offerings to remove
            var courseIds = offeringsToRemove.Select(o => o.CourseId).ToList();
            var courseTitles = await _context.Courses
                .Where(c => courseIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Title);

            foreach (var offering in offeringsToRemove)
            {
                bool hasEnrollments = await _context.Enrollments
                    .AnyAsync(e => e.CourseOfferingId == offering.Id);

                if (hasEnrollments)
                {
                    var courseTitle = courseTitles.TryGetValue(offering.CourseId, out var title)
                        ? title
                        : "Unknown Course";

                    throw new InvalidOperationException(
                        $"Cannot delete course '{courseTitle}' because students are already enrolled.");
                }

                offering.IsDeleted = true;
            }
        }

    }
}
