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
        public CurriculumService(
            UniPortalContext context,
            LogService logService)
            : base(context, logService)
        {
        }

        // Get all curriculum as DTOs
        public async Task<List<CurriculumDto>> GetAllAsync()
        {
            var query = from c in _context.Curriculums
                        join p in _context.Programs on c.ProgramId equals p.Id
                        join s in _context.Semesters on c.SemesterId equals s.Id
                        join co in _context.Courses on c.CourseId equals co.Id
                        join r in _context.CourseTypes on c.CourseTypeId equals r.Id
                        where !c.IsDeleted && !co.IsDeleted  // exclude deleted courses
                        orderby p.Name, s.SemesterType, s.AcademicYear, c.SequenceOrder
                        select new CurriculumDto
                        {
                            Id = c.Id,
                            ProgramId = p.Id,
                            ProgramName = p.Name,
                            SemesterId = s.Id,
                            SemesterName = s.SemesterType + " " + s.AcademicYear,
                            SemesterNumber = c.SemesterNumber,
                            CourseId = co.Id,
                            CourseTitle = co.Title,
                            CourseTypeId = r.Id,
                            CourseTypeName = r.Name,
                            SequenceOrder = c.SequenceOrder,
                            IsDeleted = c.IsDeleted
                        };

            return await query.ToListAsync();
        }

        // Get by Id
        public async Task<CurriculumDto?> GetByIdAsync(Guid id)
        {
            var query = from c in _context.Curriculums
                        join p in _context.Programs on c.ProgramId equals p.Id
                        join s in _context.Semesters on c.SemesterId equals s.Id
                        join co in _context.Courses on c.CourseId equals co.Id
                        join r in _context.CourseTypes on c.CourseTypeId equals r.Id
                        where c.Id == id && !c.IsDeleted && !co.IsDeleted
                        select new CurriculumDto
                        {
                            Id = c.Id,
                            ProgramId = p.Id,
                            ProgramName = p.Name,
                            SemesterId = s.Id,
                            SemesterName = s.SemesterType + " " + s.AcademicYear,
                            SemesterNumber = c.SemesterNumber,
                            CourseId = co.Id,
                            CourseTitle = co.Title,
                            CourseTypeId = r.Id,
                            CourseTypeName = r.Name,
                            SequenceOrder = c.SequenceOrder,
                            IsDeleted = c.IsDeleted
                        };

            return await query.FirstOrDefaultAsync();
        }

        // Create
        public async Task<Curriculum> CreateAsync(
            Guid programId,
            Guid semesterId,
            int semesterNumber,
            Guid courseId,
            Guid courseTypeId,
            int sequenceOrder,
            Guid? createdById)
        {
            var entity = new Curriculum
            {
                Id = Guid.NewGuid(),
                ProgramId = programId,
                SemesterId = semesterId,
                SemesterNumber = semesterNumber,
                CourseId = courseId,
                CourseTypeId = courseTypeId,
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
                new { programId, semesterId, semesterNumber, courseId, courseTypeId, sequenceOrder }
            );

            return entity;
        }

        // Update
        public async Task UpdateAsync(
            Guid id,
            Guid programId,
            Guid semesterId,
            int semesterNumber,
            Guid courseId,
            Guid courseTypeId,
            int sequenceOrder,
            Guid? updatedById)
        {
            var entity = await _context.Curriculums.FindAsync(id);
            if (entity == null) return;

            var oldValues = new
            {
                entity.ProgramId,
                entity.SemesterId,
                entity.SemesterNumber,
                entity.CourseId,
                entity.CourseTypeId,
                entity.SequenceOrder,
            };

            entity.ProgramId = programId;
            entity.SemesterId = semesterId;
            entity.SemesterNumber = semesterNumber;
            entity.CourseId = courseId;
            entity.CourseTypeId = courseTypeId;
            entity.SequenceOrder = sequenceOrder;

            await _context.SaveChangesAsync();

            await LogAsync(
                updatedById,
                ActionType.Update,
                nameof(Curriculum),
                entity.Id,
                new { Old = oldValues, New = new { programId, semesterId, semesterNumber, courseId, courseTypeId, sequenceOrder } }
            );
        }

        // Delete (soft delete)
        public async Task DeleteAsync(Guid id, Guid deletedById)
        {
            var curriculum = await _context.Curriculums.FirstOrDefaultAsync(c => c.Id == id);
            if (curriculum == null) return;

            // Check if curriculum has any active course offerings
            var hasOfferings = await _context.CourseOfferings
                .AnyAsync(co => co.CurriculumId == id && !co.IsDeleted);

            if (hasOfferings)
            {
                throw new InvalidOperationException(
                    "This curriculum cannot be deleted because it has related course offerings."
                );
            }

            // Safe to soft delete
            curriculum.IsDeleted = true;
            curriculum.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            await LogAsync(deletedById, ActionType.Delete, nameof(Curriculum), curriculum.Id);
        }


        // Activate (undo delete)
        public async Task ActivateAsync(Guid id, Guid activatedById)
        {
            var entity = await _context.Curriculums.FindAsync(id);
            if (entity == null) return;

            entity.IsDeleted = false;
            await _context.SaveChangesAsync();

            await LogAsync(activatedById, ActionType.Activate, nameof(Curriculum), entity.Id);
        }

        // Get courses for a program & semester number (excluding deleted courses)
        public async Task<List<CourseOfferingDto>> GetCurriculumCoursesAsync(Guid programId, int semesterNumber)
        {
            var query = from c in _context.Curriculums
                        join crs in _context.Courses on c.CourseId equals crs.Id
                        join ct in _context.CourseTypes on c.CourseTypeId equals ct.Id
                        where c.ProgramId == programId
                              && c.SemesterNumber == semesterNumber
                              && !c.IsDeleted
                              && !crs.IsDeleted  // exclude deleted courses
                        orderby c.SequenceOrder
                        select new CourseOfferingDto
                        {
                            CurriculumId = c.Id,
                            CourseId = c.CourseId,
                            CourseTitle = crs.Title,
                            CourseType = ct.Name,
                            CourseTypeId = ct.Id,
                            SequenceOrder = c.SequenceOrder,

                            // defaults for offering creation
                            FacultyId = Guid.Empty,
                            FacultyName = string.Empty,
                            StartTime = default,
                            EndTime = default,
                            MaxEnrollment = 0,

                            Mon = false,
                            Tue = false,
                            Wed = false,
                            Thu = false,
                            Fri = false,
                            Sat = false,
                            Sun = false
                        };

            return await query.ToListAsync();
        }
    }
}
