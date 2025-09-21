using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;

namespace UniPortal.Services.Academics.Configs
{
    public class SemesterService
    {
        private readonly UniPortalContext _context;

        public SemesterService(UniPortalContext context)
        {
            _context = context;
        }

        public async Task<List<SelectOption>> GetSelectOptionsAsync()
        {
            return await _context.Semesters
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.AcademicYear).ThenBy(s => s.SemesterType)
                .Select(s => new SelectOption
                {
                    Id = s.Id,
                    Name = $"{s.SemesterType} {s.AcademicYear}"
                })
                .ToListAsync();
        }

        public async Task<List<Semester>> GetAllAsync()
        {
            return await _context.Semesters
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.AcademicYear).ThenBy(s => s.SemesterType)
                .ToListAsync();
        }

        public async Task<Semester> GetByIdAsync(string id)
        {
            return await _context.Semesters.FirstOrDefaultAsync(s => s.Id.ToString() == id);
        }

        public async Task<Semester> GetCurrentSemesterAsync()
        {
            return await _context.Semesters.FirstOrDefaultAsync(s => s.IsCurrent && !s.IsDeleted);
        }

        public async Task<(DateTime StartDate, DateTime EndDate)> GetNextSemesterWindowAsync(int defaultDurationMonths)
        {
            // Fetch all semesters once
            var allSemesters = await GetAllAsync();

            // Find last semester by end date
            var lastSemester = allSemesters.OrderByDescending(s => s.EndDate).FirstOrDefault();

            DateTime newStartDate = lastSemester != null
                ? lastSemester.EndDate.AddDays(1)
                : DateTime.Today;

            DateTime newEndDate = newStartDate.AddMonths(defaultDurationMonths);

            return (newStartDate, newEndDate);
        }


        // --------------------- CREATE ---------------------
        public async Task<OperationResult> CreateAsync(
            string semesterType,
            string academicYear,
            DateTime startDate,
            DateTime endDate,
            bool isCurrent)
        {
            var semester = new Semester
            {
                SemesterType = semesterType,
                AcademicYear = academicYear,
                StartDate = startDate,
                EndDate = endDate,
                IsCurrent = isCurrent
            };

            return await SaveSemesterAsync(semester);
        }

        // --------------------- UPDATE ---------------------
        public async Task<OperationResult> UpdateAsync(
            Guid id,
            string semesterType,
            string academicYear,
            DateTime startDate,
            DateTime endDate,
            bool isCurrent)
        {
            var semester = await _context.Semesters.FindAsync(id);
            if (semester == null)
                return OperationResult.Fail("Semester not found.");

            semester.SemesterType = semesterType;
            semester.AcademicYear = academicYear;
            semester.StartDate = startDate;
            semester.EndDate = endDate;
            semester.IsCurrent = isCurrent;
            semester.UpdatedAt = DateTime.Now;

            return await SaveSemesterAsync(semester, id);
        }

        // --------------------- DELETE ---------------------
        public async Task DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out var semesterId))
                return;

            var semester = await _context.Semesters.FirstOrDefaultAsync(s => s.Id == semesterId);
            if (semester == null) return;

            // Check if semester has any active curriculums
            var hasCurriculums = await _context.Curriculums
                .AnyAsync(c => c.SemesterId == semesterId && !c.IsDeleted);

            // Check if semester has any active course offerings
            var hasCourseOfferings = await _context.CourseOfferings
                .AnyAsync(co => co.SemesterId == semesterId && !co.IsDeleted);

            if (hasCurriculums || hasCourseOfferings)
            {
                throw new InvalidOperationException(
                    "This semester cannot be deleted because it contains related curriculums or course offerings."
                );
            }

            // Safe to soft delete
            semester.IsDeleted = true;
            semester.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }


        // --------------------- ACTIVATE ---------------------
        public async Task ActivateAsync(string id)
        {
            var semester = await _context.Semesters.FindAsync(Guid.Parse(id));
            if (semester != null)
            {
                semester.IsDeleted = false;
                semester.DeletedAt = null;
                await _context.SaveChangesAsync();
            }
        }

        // --------------------- PRIVATE HELPER ---------------------
        private async Task<OperationResult> SaveSemesterAsync(Semester semester, Guid? ignoreId = null)
        {
            // 1️⃣ Validate Dates
            var validationResult = await ValidateSemesterDatesAsync(semester.StartDate, semester.EndDate, ignoreId);
            if (validationResult != null)
                return validationResult;

            // 2️⃣ Handle IsCurrent - ensure only one semester is current
            if (semester.IsCurrent)
            {
                var otherCurrentSemesters = await _context.Semesters
                    .Where(s => !s.IsDeleted && s.IsCurrent && (ignoreId == null || s.Id != ignoreId))
                    .ToListAsync();

                foreach (var other in otherCurrentSemesters)
                    other.IsCurrent = false;
            }

            // 3️⃣ Add or update
            if (semester.Id == Guid.Empty || !await _context.Semesters.AnyAsync(s => s.Id == semester.Id))
            {
                _context.Semesters.Add(semester);
            }
            // Else it's already tracked by EF for update

            await _context.SaveChangesAsync();

            return OperationResult.Ok("Semester saved successfully.");
        }

        // --------------------- VALIDATE ---------------------
        private async Task<OperationResult?> ValidateSemesterDatesAsync(DateTime startDate, DateTime endDate, Guid? ignoreId = null)
        {
            if (endDate <= startDate)
                return OperationResult.Fail("End date must be after start date.");

            bool overlapExists = await _context.Semesters
                .AnyAsync(s => !s.IsDeleted &&
                               (ignoreId == null || s.Id != ignoreId) &&
                               ((startDate >= s.StartDate && startDate <= s.EndDate) ||
                                (endDate >= s.StartDate && endDate <= s.EndDate) ||
                                (startDate <= s.StartDate && endDate >= s.EndDate)));

            if (overlapExists)
                return OperationResult.Fail("A semester already exists within this timeline.");

            return null;
        }
    }
}
