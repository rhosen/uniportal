using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;
using UniPortal.ViewModels.Academics;

namespace UniPortal.Services.Academics.Configs
{
    public class ClassroomService
    {
        private readonly UniPortalContext _context;

        public ClassroomService(UniPortalContext context)
        {
            _context = context;
        }

        public async Task<List<Classroom>> GetAllAsync()
        {
            return await _context.Classrooms.Where(c => !c.IsDeleted)
                .OrderBy(c => c.RoomName)
                .ToListAsync();
        }

        public async Task<Classroom> GetByIdAsync(string id)
        {
            return await _context.Classrooms
                .FirstOrDefaultAsync(c => c.Id.ToString() == id);
        }

        public async Task CreateAsync(string roomName, int capacity, string location)
        {
            var classroom = new Classroom
            {
                RoomName = roomName,
                Capacity = capacity,
                Location = location
            };
            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, string roomName, int capacity, string location)
        {
            var classroom = await _context.Classrooms.FindAsync(id);
            if (classroom != null)
            {
                classroom.RoomName = roomName;
                classroom.Capacity = capacity;
                classroom.Location = location
                    ;
                classroom.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(string id)
        {
            var classroom = await _context.Classrooms.FindAsync(Guid.Parse(id));
            if (classroom != null)
            {
                classroom.IsDeleted = true;
                classroom.DeletedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ActivateAsync(string id)
        {
            var classroom = await _context.Classrooms.FindAsync(Guid.Parse(id));
            if (classroom != null)
            {
                classroom.IsDeleted = false;
                classroom.DeletedAt = null;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<ClassroomAvailabilityViewModel>> GetClassroomAvailabilityAsync()
        {
            var now = DateTime.Now;
            var currentDay = (int)now.DayOfWeek; // 0 = Sunday … 6 = Saturday
            var currentTime = TimeOnly.FromDateTime(now);

            // Get current semester
            var semesterId = await _context.Semesters
                .Where(s => s.StartDate <= now && s.EndDate >= now && !s.IsDeleted)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            // Fetch schedules for today
            var schedulesToday = await _context.ClassScheduleEntries
                .Where(e => !e.IsDeleted &&
                            !e.Schedule.IsDeleted &&
                            e.Schedule.Course.SemesterId == semesterId &&
                            e.DayOfWeek == currentDay)
                .Include(e => e.Schedule)
                    .ThenInclude(cs => cs.Course)
                        .ThenInclude(c => c.Subject)
                .Include(e => e.Schedule)
                    .ThenInclude(cs => cs.Course)
                        .ThenInclude(c => c.Teacher)
                .Include(e => e.Schedule)
                    .ThenInclude(cs => cs.Classroom)
                .ToListAsync();

            // Filter schedules that are currently ongoing, covering overnight
            var ongoingSchedules = schedulesToday.Where(e =>
                (e.StartTime <= e.EndTime && e.StartTime <= currentTime && e.EndTime >= currentTime) || // normal
                (e.StartTime > e.EndTime && (currentTime >= e.StartTime || currentTime <= e.EndTime))   // overnight
            ).ToList();

            // Fetch all classrooms
            var classrooms = await _context.Classrooms
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            // Map classrooms with current schedules
            var result = classrooms.Select(c =>
            {
                var currentSchedules = ongoingSchedules
                    .Where(e => e.Schedule.ClassroomId == c.Id)
                    .Select(e => new ScheduleInfo
                    {
                        SubjectCode = e.Schedule.Course.Subject.Code,
                        SubjectName = e.Schedule.Course.Subject.Name,
                        StartTime = e.StartTime,
                        EndTime = e.EndTime
                    })
                    .ToList();

                return new ClassroomAvailabilityViewModel
                {
                    Id = c.Id,
                    RoomName = c.RoomName,
                    Location = c.Location,
                    Capacity = c.Capacity,
                    IsOccupied = currentSchedules.Any(),
                    CurrentSchedules = currentSchedules
                };
            }).ToList();

            return result;
        }
    }
}
