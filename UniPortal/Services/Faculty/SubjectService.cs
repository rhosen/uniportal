using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities;

namespace UniPortal.Services.Faculty
{
    public class SubjectService : BaseService<Subject>
    {
        public SubjectService(UniPortalContext context, LogService logService)
            : base(context, logService)
        {
        }

        public async Task<List<Subject>> GetAllAsync()
        {
            return await _context.Subjects
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Subject?> GetByIdAsync(Guid subjectId)
        {
            return await _context.Subjects
                .FirstOrDefaultAsync(s => s.Id == subjectId && !s.IsDeleted);
        }

        public async Task<Subject> CreateAsync(string code, string name, Guid? createdById)
        {
            var subject = new Subject
            {
                Code = code,
                Name = name,
                CreatedById = createdById,
                CreatedAt = DateTime.UtcNow
            };

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            // Logging using BaseService method
            await LogAsync(
                createdById,
                ActionType.Create,
                "Subject",
                subject.Id,
                new { Code = code, Name = name }
            );

            return subject;
        }

        public async Task UpdateAsync(Guid subjectId, string code, string name, Guid? updatedById)
        {
            var subject = await _context.Subjects.FindAsync(subjectId);
            if (subject == null) return;

            var oldValues = new { subject.Code, subject.Name };

            subject.Code = code;
            subject.Name = name;
            subject.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await LogAsync(
                updatedById,
                ActionType.Update,
                "Subject",
                subject.Id,
                new { Old = oldValues, New = new { Code = code, Name = name } }
            );
        }

        public async Task DeleteAsync(Guid subjectId, Guid deletedById)
        {
            var subject = await _context.Subjects.FindAsync(subjectId);
            if (subject == null) return;

            subject.IsDeleted = true;
            subject.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await LogAsync(deletedById, ActionType.Delete, "Subject", subject.Id);
        }

        public async Task ActivateAsync(Guid subjectId, Guid activatedById)
        {
            var subject = await _context.Subjects.FindAsync(subjectId);
            if (subject == null) return;

            subject.IsDeleted = false;
            subject.DeletedAt = null;

            await _context.SaveChangesAsync();

            await LogAsync(activatedById, ActionType.Activate, "Subject", subject.Id);
        }
    }
}
