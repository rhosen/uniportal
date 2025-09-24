using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;

namespace UniPortal.Services.Academics.Configs
{
    public class GradeScaleService
    {
        private readonly UniPortalContext _context;

        public GradeScaleService(UniPortalContext context)
        {
            _context = context;
        }

        public async Task<List<GradeScale>> GetAllAsync()
        {
            return await _context.GradeScales
                .OrderByDescending(g => g.MinMarks)
                .ToListAsync();
        }

        public async Task<GradeScale?> GetByIdAsync(Guid id)
        {
            return await _context.GradeScales.FindAsync(id);
        }

        public async Task UpdateAsync(Guid id, string grade, decimal minMarks, decimal maxMarks, decimal gpa)
        {
            // Validate overlapping ranges
            bool isOverlap = await _context.GradeScales
                .Where(g => g.Id != id)
                .AnyAsync(g => !(maxMarks < g.MinMarks || minMarks > g.MaxMarks));

            if (isOverlap)
                throw new InvalidOperationException("The mark range overlaps with another grade scale.");

            var scale = await _context.GradeScales.FindAsync(id);
            if (scale == null) return;

            scale.Grade = grade;
            scale.MinMarks = minMarks;
            scale.MaxMarks = maxMarks;
            scale.GPA = gpa;
            scale.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }
}
