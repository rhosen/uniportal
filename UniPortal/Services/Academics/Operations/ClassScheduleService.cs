using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;
using UniPortal.Services.Infrastructures;
using UniPortal.ViewModels.Classes;

namespace UniPortal.Services.Academics.Operations
{
    public class ClassScheduleService : BaseService<ClassSchedule>
    {
        public ClassScheduleService(UniPortalContext context, LogService logService)
            : base(context, logService)
        {
        }


        public async Task<ScheduleInputModel?> GetScheduleByIdAsync(Guid scheduleId)
        {
            var schedule = await _context.ClassSchedules
                .Where(cs => cs.Id == scheduleId && !cs.IsDeleted)
                .Select(cs => new ScheduleInputModel
                {
                    ScheduleId = cs.Id,
                    SelectedCourseId = cs.CourseId,
                    SelectedClassroomId = cs.ClassroomId,
                    SelectedDays = cs.Entries
                        .Where(e => !e.IsDeleted)
                        .Select(e => e.DayOfWeek)
                        .ToList(),
                    StartTime = cs.Entries
                        .Where(e => !e.IsDeleted)
                        .Select(e => e.StartTime)
                        .FirstOrDefault(),
                    EndTime = cs.Entries
                        .Where(e => !e.IsDeleted)
                        .Select(e => e.EndTime)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            return schedule;
        }



        // ======================
        // Get schedules (for display)
        // ======================
        public async Task<List<ScheduleViewModel>> GetSchedulesAsync(string searchTerm = "")
        {
            var schedules = await _context.ClassSchedules
                .Include(cs => cs.Course)
                    .ThenInclude(c => c.Subject)
                .Include(cs => cs.Course)
                    .ThenInclude(c => c.Department)
                .Include(cs => cs.Course)
                    .ThenInclude(c => c.Teacher)
                .Include(cs => cs.Classroom)
                .Where(cs => !cs.IsDeleted)
                .ToListAsync();

            // Map to view model
            var result = schedules.Select(cs => new ScheduleViewModel
            {
                ScheduleId = cs.Id,
                CourseName = $"{cs.Course.Department.Code} · {cs.Course.Subject.Name} ({cs.Course.Subject.Code}) · {cs.Course.Teacher.FirstName} {cs.Course.Teacher.LastName}",
                ClassroomName = cs.Classroom.RoomName,
                Entries = _context.ClassScheduleEntries
                    .Where(e => e.ScheduleId == cs.Id && !e.IsDeleted)
                    .Select(e => new ScheduleEntryViewModel
                    {
                        EntryId = e.Id,
                        DayOfWeek = e.DayOfWeek,
                        StartTime = e.StartTime,
                        EndTime = e.EndTime
                    })
                    .OrderBy(e => e.DayOfWeek)
                    .ToList()
            }).ToList();

            // Filter by search term
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                result = result
                    .Where(s => s.CourseName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                s.ClassroomName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return result;
        }

        // ======================
        // Get courses for dropdown
        // ======================
        public async Task<List<SelectOption>> GetCoursesForDropdownAsync()
        {
            var today = DateTime.Today;

            return await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Department)
                .Include(c => c.Teacher)
                .Include(c => c.Semester)
                .Where(c => c.Semester.StartDate <= today && today <= c.Semester.EndDate)
                .Select(c => new SelectOption
                {
                    Id = c.Id,
                    Name = $"{c.Semester.Name} · {c.Department.Code} · {c.Subject.Name} ({c.Subject.Code}) · {c.Teacher.FirstName} {c.Teacher.LastName}"
                })
                .ToListAsync();
        }



        // ======================
        // Get classrooms for dropdown
        // ======================
        public async Task<List<SelectOption>> GetClassroomsForDropdownAsync()
        {
            return await _context.Classrooms
                .Select(c => new SelectOption
                {
                    Id = c.Id,
                    Name = c.RoomName
                })
                .ToListAsync();
        }

        // ======================
        // Create schedule (multiple entries)
        // ======================
        public async Task CreateScheduleAsync(
            Guid courseId,
            Guid classroomId,
            List<int> days,
            TimeOnly startTime,
            TimeOnly endTime,
            Guid? createdById = null)
        {
            var schedule = new ClassSchedule
            {
                CourseId = courseId,
                ClassroomId = classroomId,
                CreatedAt = DateTime.Now,
                CreatedById = createdById
            };
            _context.ClassSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            foreach (var day in days)
            {
                var entry = new ClassScheduleEntry
                {
                    ScheduleId = schedule.Id,
                    DayOfWeek = day,
                    StartTime = startTime,
                    EndTime = endTime,
                    CreatedAt = DateTime.Now,
                    CreatedById = createdById
                };
                _context.ClassScheduleEntries.Add(entry);
            }

            await _context.SaveChangesAsync();

            await LogAsync(createdById, Constants.ActionType.Create, "ClassSchedule", schedule.Id, new { courseId, classroomId, days, startTime, endTime });
        }

        // ======================
        // Update schedule (multiple entries)
        // ======================
        public async Task UpdateScheduleAsync(
            Guid scheduleId,
            Guid courseId,
            Guid classroomId,
            List<int> days,
            TimeOnly startTime,
            TimeOnly endTime,
            Guid? updatedById = null)
        {
            var schedule = await _context.ClassSchedules.FindAsync(scheduleId);
            if (schedule == null) return;

            schedule.CourseId = courseId;
            schedule.ClassroomId = classroomId;
            schedule.UpdatedAt = DateTime.Now;

            // Get all existing entries for this schedule
            var existingEntries = await _context.ClassScheduleEntries
                .Where(e => e.ScheduleId == scheduleId && !e.IsDeleted)
                .ToListAsync();

            // Soft-delete entries that are no longer selected
            foreach (var entry in existingEntries)
            {
                if (!days.Contains(entry.DayOfWeek))
                {
                    entry.IsDeleted = true;
                    entry.UpdatedAt = DateTime.Now;
                }
            }

            // Update existing or create new entries for selected days
            foreach (var day in days)
            {
                var entry = existingEntries.FirstOrDefault(e => e.DayOfWeek == day && !e.IsDeleted);
                if (entry != null)
                {
                    // Update existing entry
                    entry.StartTime = startTime;
                    entry.EndTime = endTime;
                    entry.UpdatedAt = DateTime.Now;
                }
                else
                {
                    // Create new entry
                    var newEntry = new ClassScheduleEntry
                    {
                        ScheduleId = scheduleId,
                        DayOfWeek = day,
                        StartTime = startTime,
                        EndTime = endTime,
                        CreatedAt = DateTime.Now,
                        CreatedById = updatedById
                    };
                    _context.ClassScheduleEntries.Add(newEntry);
                }
            }

            await _context.SaveChangesAsync();

            await LogAsync(updatedById, Constants.ActionType.Update, "ClassSchedule", schedule.Id,
                new { courseId, classroomId, days, startTime, endTime });
        }

        // ======================
        // Delete schedule (soft delete)
        // ======================
        public async Task DeleteScheduleAsync(Guid scheduleId, Guid? deletedById = null)
        {
            var schedule = await _context.ClassSchedules.FindAsync(scheduleId);
            if (schedule == null) return;

            schedule.IsDeleted = true;
            schedule.DeletedAt = DateTime.Now;

            var entries = await _context.ClassScheduleEntries
                .Where(e => e.ScheduleId == scheduleId && !e.IsDeleted)
                .ToListAsync();

            foreach (var entry in entries)
            {
                entry.IsDeleted = true;
                entry.DeletedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            await LogAsync(deletedById, Constants.ActionType.Delete, "ClassSchedule", schedule.Id);
        }
    }
}
