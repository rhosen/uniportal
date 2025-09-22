using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;

namespace UniPortal.Services.Academics.Configs
{
    public class SemesterService
    {
        private readonly UniPortalContext _context;
        private readonly IConfiguration _configuration;

        public SemesterService(UniPortalContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<List<SelectOption>> GetSelectOptionsAsync()
        {
            return await _context.Semesters
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.AcademicYear)
                .ThenBy(s => s.SemesterType)
                .Select(s => new SelectOption
                {
                    Id = s.Id,
                    Name = s.IsCurrent ? $"{s.SemesterType} {s.AcademicYear} (Current)"
                                       : $"{s.SemesterType} {s.AcademicYear}"
                })
                .ToListAsync();
        }



        public List<SemesterOption> GetSemesterNumberOptions()
        {
            int totalSemesters = _configuration.GetValue<int>("TotalSemesters", 8);

            return Enumerable.Range(1, totalSemesters)
                .Select(n => new SemesterOption
                {
                    Number = n,
                    Name = $"Semester {n}"
                })
                .ToList();
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


            // Check if semester has any active course offerings
            var hasCourseOfferings = await _context.CourseOfferings
                .AnyAsync(co => co.SemesterId == semesterId && !co.IsDeleted);

            if (hasCourseOfferings)
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

        private async Task<OperationResult> SaveSemesterAsync(Semester semester, Guid? ignoreId = null)
        {
            if (semester == null)
                throw new ArgumentNullException(nameof(semester));

            // 1️⃣ Validation
            var validationResult = await ValidateSemesterAsync(semester, ignoreId);
            if (validationResult != null)
                return validationResult;

            // 2️⃣ Unmark other current semesters if needed
            if (semester.IsCurrent)
                await UnmarkOtherCurrentSemestersAsync(semester.Id);

            // 3️⃣ Add or update
            if (semester.Id == Guid.Empty)
                semester.Id = Guid.NewGuid();

            var trackedSemester = await _context.Semesters.FindAsync(semester.Id);
            if (trackedSemester == null)
                _context.Semesters.Add(semester);
            else
                _context.Entry(trackedSemester).CurrentValues.SetValues(semester);

            await _context.SaveChangesAsync();

            return OperationResult.Ok("Semester saved successfully.");
        }

        private async Task UnmarkOtherCurrentSemestersAsync(Guid semesterId)
        {
            var otherCurrentSemesters = await _context.Semesters
                .Where(s => !s.IsDeleted && s.IsCurrent && s.Id != semesterId)
                .ToListAsync();

            foreach (var other in otherCurrentSemesters)
                other.IsCurrent = false;
        }


        private async Task<OperationResult> ValidateSemesterAsync(Semester semester, Guid? ignoreId = null)
        {
            if (semester.EndDate <= semester.StartDate)
                return OperationResult.Fail("End date must be after start date.");

            // Check for overlapping semesters
            bool overlapExists = await _context.Semesters
                .AnyAsync(s => !s.IsDeleted &&
                               (ignoreId == null || s.Id != ignoreId) &&
                               ((semester.StartDate >= s.StartDate && semester.StartDate <= s.EndDate) ||
                                (semester.EndDate >= s.StartDate && semester.EndDate <= s.EndDate) ||
                                (semester.StartDate <= s.StartDate && semester.EndDate >= s.EndDate)));

            if (overlapExists)
                return OperationResult.Fail("A semester already exists within this timeline.");

            // ✅ Validate IsCurrent
            if (semester.IsCurrent)
            {
                var today = DateTime.Today;
                if (semester.StartDate > today || semester.EndDate < today)
                    return OperationResult.Fail(
                        "Cannot mark this semester as current because today's date is outside its start/end range.");
            }

            return null;
        }
    }
}
