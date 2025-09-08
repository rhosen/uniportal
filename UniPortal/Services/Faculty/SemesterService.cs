using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;

namespace UniPortal.Services.Faculty
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

        public async Task CreateAsync(string name, DateTime startDate, DateTime endDate)
        {
            var semester = new Semester
            {
                Name = name,
                StartDate = startDate,
                EndDate = endDate
            };
            _context.Semesters.Add(semester);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, string name, DateTime startDate, DateTime endDate)
        {
            var semester = await _context.Semesters.FindAsync(id);
            if (semester != null)
            {
                semester.Name = name;
                semester.StartDate = startDate;
                semester.EndDate = endDate;
                semester.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
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
