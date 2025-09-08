using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Services.Infrastructures;

namespace UniPortal.Services.Academics.Operations
{
    public class ClassScheduleService : BaseService<ClassSchedule>
    {
        public ClassScheduleService(UniPortalContext context, LogService logService)
            : base(context, logService)
        {
        }

        // ======================
        // Get All Schedules
        // ======================
        public async Task<List<ClassSchedule>> GetAllAsync()
        {
            return await _context.ClassSchedules
                .Include(cs => cs.Course)
                    .ThenInclude(c => c.Subject)
                .Include(cs => cs.Course)
                    .ThenInclude(c => c.Department)
                .Include(cs => cs.Course)
                    .ThenInclude(c => c.Teacher)
                .Include(cs => cs.Course)
                    .ThenInclude(c => c.Semester)
                .Include(cs => cs.Classroom)
                .Where(cs => !cs.IsDeleted)
                .OrderBy(cs => cs.DayOfWeek)
                .ThenBy(cs => cs.StartTime)
                .ToListAsync();
        }

        // ======================
        // Get Schedule by Id
        // ======================
        public async Task<ClassSchedule> GetByIdAsync(string id)
        {
            return await _context.ClassSchedules
                .Include(cs => cs.Course)
                    .ThenInclude(c => c.Subject)
                .Include(cs => cs.Course)
                    .ThenInclude(c => c.Department)
                .Include(cs => cs.Course)
                    .ThenInclude(c => c.Teacher)
                .Include(cs => cs.Course)
                    .ThenInclude(c => c.Semester)
                .Include(cs => cs.Classroom)
                .FirstOrDefaultAsync(cs => cs.Id.ToString() == id && !cs.IsDeleted);
        }

        // ======================
        // Create New Schedule
        // ======================
        public async Task<ClassSchedule> CreateAsync(Guid courseId, Guid classroomId, int dayOfWeek,
            TimeOnly startTime, TimeOnly endTime, Guid? createdById = null)
        {
            var sched = new ClassSchedule
            {
                CourseId = courseId,
                ClassroomId = classroomId,
                DayOfWeek = dayOfWeek,
                StartTime = startTime,
                EndTime = endTime,
                CreatedAt = DateTime.Now,
                CreatedById = createdById
            };

            _context.ClassSchedules.Add(sched);
            await _context.SaveChangesAsync();

            await LogAsync(createdById, ActionType.Create, "ClassSchedule", sched.Id,
                new { courseId, classroomId, dayOfWeek, startTime, endTime });

            return sched;
        }

        // ======================
        // Update Schedule
        // ======================
        public async Task UpdateAsync(string id, Guid courseId, Guid classroomId, int dayOfWeek,
            TimeOnly startTime, TimeOnly endTime, Guid? updatedById = null)
        {
            var sched = await _context.ClassSchedules.FindAsync(Guid.Parse(id));
            if (sched == null) return;

            var oldValues = new
            {
                sched.CourseId,
                sched.ClassroomId,
                sched.DayOfWeek,
                sched.StartTime,
                sched.EndTime
            };

            sched.CourseId = courseId;
            sched.ClassroomId = classroomId;
            sched.DayOfWeek = dayOfWeek;
            sched.StartTime = startTime;
            sched.EndTime = endTime;
            sched.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            await LogAsync(updatedById, ActionType.Update, "ClassSchedule", sched.Id,
                new { Old = oldValues, New = new { courseId, classroomId, dayOfWeek, startTime, endTime } });
        }

        // ======================
        // Delete (Soft Delete)
        // ======================
        public async Task DeleteAsync(string id, Guid? deletedById = null)
        {
            var sched = await _context.ClassSchedules.FindAsync(Guid.Parse(id));
            if (sched == null) return;

            sched.IsDeleted = true;
            sched.DeletedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            await LogAsync(deletedById, ActionType.Delete, "ClassSchedule", sched.Id);
        }
    }
}
