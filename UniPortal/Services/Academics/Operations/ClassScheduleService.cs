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
            var query =
                from schedule in _unitOfWork.Context.Schedules
                join course in _unitOfWork.Context.Courses on schedule.CourseId equals course.Id
                join subject in _unitOfWork.Context.Subjects on course.SubjectId equals subject.Id
                join teacher in _unitOfWork.Context.Accounts on course.TeacherId equals teacher.Id
                join semester in _unitOfWork.Context.Semesters on course.SemesterId equals semester.Id
                join program in _unitOfWork.Context.Programs on semester.ProgramId equals program.Id
                join room in _unitOfWork.Context.Rooms on schedule.RoomId equals room.Id
                where !schedule.IsDeleted
                select new
                {
                    Schedule = schedule,
                    Course = course,
                    Subject = subject,
                    Teacher = teacher,
                    Semester = semester,
                    Program = program,
                    Room = room
                };

            var schedules = await query.ToListAsync();

            var result = schedules.Select(cs => new ScheduleViewModel
            {
                ScheduleId = cs.Schedule.Id,
                CourseName =
                    $"{cs.Program.Code} · {cs.Subject.Name} ({cs.Subject.Code}) · {cs.Teacher.FirstName} {cs.Teacher.LastName}",
                ClassroomName = cs.Room.RoomName,
                Sessions = _unitOfWork.Context.Sessions
                    .Where(e => e.ScheduleId == cs.Schedule.Id && !e.IsDeleted)
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
                    .Where(s =>
                        s.CourseName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
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
                .Include(c => c.Teacher)
                .Include(c => c.Semester)
                .Select(c => new SelectOption
                {
                    Id = c.Id,
                    Name = $"{c.Semester.Name} · {c.Subject.Name} ({c.Subject.Code}) · {c.Teacher.FirstName} {c.Teacher.LastName}"
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
