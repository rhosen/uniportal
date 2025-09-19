using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;
using UniPortal.Services.Infrastructures;
using UniPortal.ViewModels.Classes;

namespace UniPortal.Services.Academics.Operations
{
    public class ClassScheduleService : BaseService<Schedule>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClassScheduleService(IUnitOfWork unitOfWork, LogService logService)
            : base(unitOfWork.Context, logService)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ScheduleInputModel?> GetScheduleByIdAsync(Guid scheduleId)
        {
            var schedule = await _unitOfWork.Context.Schedules
                .Where(cs => cs.Id == scheduleId && !cs.IsDeleted)
                .Select(cs => new ScheduleInputModel
                {
                    ScheduleId = cs.Id,
                    SelectedCourseId = cs.CourseId,
                    SelectedClassroomId = cs.RoomId,
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

        public async Task<List<ScheduleViewModel>> GetSchedulesAsync(string searchTerm = "")
        {
            var schedules = await _unitOfWork.Context.Schedules
                .Include(cs => cs.Course)
                    .ThenInclude(c => c.Subject)
                .Include(cs => cs.Course)
                    .ThenInclude(c => c.Department)
                .Include(cs => cs.Course)
                    .ThenInclude(c => c.Teacher)
                .Include(cs => cs.Room)
                .Where(cs => !cs.IsDeleted)
                .ToListAsync();

            var result = schedules.Select(cs => new ScheduleViewModel
            {
                ScheduleId = cs.Id,
                CourseName = $"{cs.Course.Department.Code} · {cs.Course.Subject.Name} ({cs.Course.Subject.Code}) · {cs.Course.Teacher.FirstName} {cs.Course.Teacher.LastName}",
                ClassroomName = cs.Room.RoomName,
                Sessions = _unitOfWork.Context.Sessions
                    .Where(e => e.ScheduleId == cs.Id && !e.IsDeleted)
                    .Select(e => new SessionViewModel
                    {
                        EntryId = e.Id,
                        DayOfWeek = e.DayOfWeek,
                        StartTime = e.StartTime,
                        EndTime = e.EndTime
                    })
                    .OrderBy(e => e.DayOfWeek)
                    .ToList()
            }).ToList();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                result = result
                    .Where(s => s.CourseName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                s.ClassroomName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return result;
        }

        public async Task<List<SelectOption>> GetCoursesForDropdownAsync()
        {
            var today = DateTime.Today;

            return await _unitOfWork.Context.Courses
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

        public async Task<List<SelectOption>> GetClassroomsForDropdownAsync()
        {
            return await _unitOfWork.Context.Rooms
                .Select(c => new SelectOption
                {
                    Id = c.Id,
                    Name = c.RoomName
                })
                .ToListAsync();
        }

        public async Task CreateScheduleAsync(
            Guid courseId,
            Guid classroomId,
            List<int> days,
            TimeOnly startTime,
            TimeOnly endTime,
            Guid? createdById = null)
        {
            var schedule = new Schedule
            {
                CourseId = courseId,
                RoomId = classroomId,
                CreatedAt = DateTime.Now,
                ModifiedById = createdById
            };
            _unitOfWork.Context.Schedules.Add(schedule);

            foreach (var day in days)
            {
                var entry = new Session
                {
                    ScheduleId = schedule.Id,
                    DayOfWeek = day,
                    StartTime = startTime,
                    EndTime = endTime,
                    CreatedAt = DateTime.Now,
                    ModifiedById = createdById
                };
                _unitOfWork.Context.Sessions.Add(entry);
            }

            await _unitOfWork.CommitAsync();

            await LogAsync(createdById, ActionType.Create, "ClassSchedule", schedule.Id, new { courseId, classroomId, days, startTime, endTime });
        }

        public async Task UpdateScheduleAsync(
            Guid scheduleId,
            Guid courseId,
            Guid classroomId,
            List<int> days,
            TimeOnly startTime,
            TimeOnly endTime,
            Guid? updatedById = null)
        {
            var schedule = await _unitOfWork.Context.Schedules.FindAsync(scheduleId);
            if (schedule == null) return;

            schedule.CourseId = courseId;
            schedule.RoomId = classroomId;
            schedule.UpdatedAt = DateTime.Now;

            var existingEntries = await _unitOfWork.Context.Sessions
                .Where(e => e.ScheduleId == scheduleId && !e.IsDeleted)
                .ToListAsync();

            foreach (var entry in existingEntries)
            {
                if (!days.Contains(entry.DayOfWeek))
                {
                    entry.IsDeleted = true;
                    entry.UpdatedAt = DateTime.Now;
                }
            }

            foreach (var day in days)
            {
                var entry = existingEntries.FirstOrDefault(e => e.DayOfWeek == day && !e.IsDeleted);
                if (entry != null)
                {
                    entry.StartTime = startTime;
                    entry.EndTime = endTime;
                    entry.UpdatedAt = DateTime.Now;
                }
                else
                {
                    var newEntry = new Session
                    {
                        ScheduleId = scheduleId,
                        DayOfWeek = day,
                        StartTime = startTime,
                        EndTime = endTime,
                        CreatedAt = DateTime.Now,
                        ModifiedById = updatedById
                    };
                    _unitOfWork.Context.Sessions.Add(newEntry);
                }
            }

            await _unitOfWork.CommitAsync();

            await LogAsync(updatedById, ActionType.Update, "ClassSchedule", schedule.Id,
                new { courseId, classroomId, days, startTime, endTime });
        }

        public async Task DeleteScheduleAsync(Guid scheduleId, Guid? deletedById = null)
        {
            var schedule = await _unitOfWork.Context.Schedules.FindAsync(scheduleId);
            if (schedule == null) return;

            schedule.IsDeleted = true;
            schedule.DeletedAt = DateTime.Now;

            var entries = await _unitOfWork.Context.Sessions
                .Where(e => e.ScheduleId == scheduleId && !e.IsDeleted)
                .ToListAsync();

            foreach (var entry in entries)
            {
                entry.IsDeleted = true;
                entry.DeletedAt = DateTime.Now;
            }

            await _unitOfWork.CommitAsync();

            await LogAsync(deletedById, ActionType.Delete, "ClassSchedule", schedule.Id);
        }
    }
}
