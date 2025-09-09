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

        public async Task<List<Semester>> GetAllAsync()
        {
            return await _context.Semesters.Where(x=> !x.IsDeleted)
                .OrderBy(s => s.StartDate)
                .ToListAsync();
        }

        public async Task<List<Semester>> GetOnGoingSemestersAsync()
        {
            var currentDate = DateTime.Now;

            return await _context.Semesters
                .Where(s => !s.IsDeleted && s.EndDate > currentDate)
                .OrderBy(s => s.StartDate)
                .ToListAsync();
        }


        public async Task<Semester> GetByIdAsync(string id)
        {
            return await _context.Semesters
                .FirstOrDefaultAsync(s => s.Id.ToString() == id);
        }

        public async Task<OperationResult> CreateAsync(string name, DateTime startDate, DateTime endDate)
        {
            var validationResult = await ValidateSemesterDatesAsync(startDate, endDate);
            if (validationResult != null)
                return validationResult;

            var semester = new Data.Entities.Semester
            {
                Name = name,
                StartDate = startDate,
                EndDate = endDate
            };

            _context.Semesters.Add(semester);
            await _context.SaveChangesAsync();

            return OperationResult.Ok("Semester created successfully.");
        }


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

            return null; // null = validation passed
        }


        public async Task<OperationResult> UpdateAsync(Guid id, string name, DateTime startDate, DateTime endDate)
        {
            var validationResult = await ValidateSemesterDatesAsync(startDate, endDate, id);
            if (validationResult != null)
                return validationResult;

            var semester = await _context.Semesters.FindAsync(id);
            if (semester == null)
                return OperationResult.Fail("Semester not found.");

            semester.Name = name;
            semester.StartDate = startDate;
            semester.EndDate = endDate;
            semester.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return OperationResult.Ok("Semester updated successfully.");
        }


        public async Task DeleteAsync(string id)
        {
            var semester = await _context.Semesters.FindAsync(Guid.Parse(id));
            if (semester != null)
            {
                semester.IsDeleted = true;
                semester.DeletedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

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
    }
}
