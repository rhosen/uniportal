using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;
using UniPortal.Services.Infrastructures;

namespace UniPortal.Services.Academics.Configs
{
    public class CurriculumService : BaseService<Curriculum>
    {
        public CurriculumService(UniPortalContext context, LogService logService)
            : base(context, logService)
        {
        }

        // -----------------------------
        // Get all curricula
        // -----------------------------
        public async Task<List<CurriculumDto>> GetAllAsync()
        {
            var query = from c in _context.Curriculums
                        join p in _context.Programs on c.ProgramId equals p.Id
                        join crs in _context.Courses on c.CourseId equals crs.Id
                        join ct in _context.CourseTypes on crs.CourseTypeId equals ct.Id
                        where !c.IsDeleted && !crs.IsDeleted
                        orderby p.Name, c.SemesterNumber, c.SequenceOrder
                        select new CurriculumDto
                        {
                            Id = c.Id,
                            ProgramId = p.Id,
                            ProgramName = p.Name,
                            SemesterNumber = c.SemesterNumber,
                            CourseId = crs.Id,
                            CourseTitle = crs.Title,
                            SequenceOrder = c.SequenceOrder,
                            IsDeleted = c.IsDeleted
                        };

            return await query.ToListAsync();
        }

        // -----------------------------
        // Get curriculum by Id
        // -----------------------------
        public async Task<CurriculumDto?> GetByIdAsync(Guid id)
        {
            var query = from c in _context.Curriculums
                        join p in _context.Programs on c.ProgramId equals p.Id
                        join crs in _context.Courses on c.CourseId equals crs.Id
                        join ct in _context.CourseTypes on crs.CourseTypeId equals ct.Id
                        where c.Id == id && !c.IsDeleted && !crs.IsDeleted
                        select new CurriculumDto
                        {
                            Id = c.Id,
                            ProgramId = p.Id,
                            ProgramName = p.Name,
                            SemesterNumber = c.SemesterNumber,
                            CourseId = crs.Id,
                            CourseTitle = crs.Title,
                            SequenceOrder = c.SequenceOrder,
                            IsDeleted = c.IsDeleted
                        };

            return await query.FirstOrDefaultAsync();
        }

        // -----------------------------
        // Create new curriculum entry
        // -----------------------------
        public async Task<Curriculum> CreateAsync(
            Guid programId, int semesterNumber,
            Guid courseId, int sequenceOrder, Guid? createdById)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                throw new InvalidOperationException("Course not found.");

            var entity = new Curriculum
            {
                Id = Guid.NewGuid(),
                ProgramId = programId,
                SemesterNumber = semesterNumber,
                CourseId = courseId,
                SequenceOrder = sequenceOrder,
                IsDeleted = false
            };

            _context.Curriculums.Add(entity);
            await _context.SaveChangesAsync();

            await LogAsync(
                createdById,
                ActionType.Create,
                nameof(Curriculum),
                entity.Id,
                new { programId, semesterNumber, courseId, sequenceOrder }
            );

            return entity;
        }

        // -----------------------------
        // Update existing curriculum
        // -----------------------------
        public async Task UpdateAsync(
            Guid id, Guid programId, int semesterNumber,
            Guid courseId, int sequenceOrder, Guid? updatedById)
        {
            var entity = await _context.Curriculums.FindAsync(id);
            if (entity == null) return;

            var oldValues = new
            {
                entity.ProgramId,
                entity.SemesterNumber,
                entity.CourseId,
                entity.SequenceOrder
            };

            entity.ProgramId = programId;
            entity.SemesterNumber = semesterNumber;
            entity.CourseId = courseId;
            entity.SequenceOrder = sequenceOrder;

            await _context.SaveChangesAsync();

            await LogAsync(
                updatedById,
                ActionType.Update,
                nameof(Curriculum),
                entity.Id,
                new { Old = oldValues, New = new { programId, semesterNumber, courseId, sequenceOrder } }
            );
        }

        // -----------------------------
        // Soft delete
        // -----------------------------
        public async Task DeleteAsync(Guid id, Guid deletedById)
        {
            var curriculum = await _context.Curriculums.FirstOrDefaultAsync(c => c.Id == id);
            if (curriculum == null) return;

            var hasOfferings = await _context.CourseOfferings
                .AnyAsync(co => co.CurriculumId == id && !co.IsDeleted);

            if (hasOfferings)
                throw new InvalidOperationException("Cannot delete curriculum with active course offerings.");

            curriculum.IsDeleted = true;
            curriculum.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            await LogAsync(deletedById, ActionType.Delete, nameof(Curriculum), curriculum.Id);
        }

        // -----------------------------
        // Reactivate curriculum
        // -----------------------------
        public async Task ActivateAsync(Guid id, Guid activatedById)
        {
            var entity = await _context.Curriculums.FindAsync(id);
            if (entity == null) return;

            entity.IsDeleted = false;
            await _context.SaveChangesAsync();

            await LogAsync(activatedById, ActionType.Activate, nameof(Curriculum), entity.Id);
        }
    }
}
