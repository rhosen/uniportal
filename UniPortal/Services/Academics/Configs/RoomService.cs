using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;

namespace UniPortal.Services.Academics.Configs
{
    public class RoomService
    {
        private readonly UniPortalContext _context;

        public RoomService(UniPortalContext context)
        {
            _context = context;
        }

        // Get all rooms (regardless of IsClassroom)
        public async Task<List<Room>> GetAllAsync()
        {
            return await _context.Rooms
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.RoomName)
                .ToListAsync();
        }

        // Get rooms that are classrooms only
        public async Task<List<SelectOption>> GetOptionsAsync()
        {
            return await _context.Rooms
                .Where(r => !r.IsDeleted && r.IsClassroom) // only classrooms
                .OrderBy(r => r.RoomName)
                .Select(r => new SelectOption
                {
                    Id = r.Id,
                    Name = r.RoomName
                })
                .ToListAsync();
        }

        // Get room by Id
        public async Task<Room?> GetByIdAsync(Guid id) // use Guid directly
        {
            return await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        // Create a new classroom
        public async Task CreateAsync(string roomName, int capacity, string location, bool isClassroom = true)
        {
            var room = new Room
            {
                RoomName = roomName,
                Capacity = capacity,
                Location = location,
                IsClassroom = isClassroom
            };
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
        }

        // Update room info
        public async Task UpdateAsync(Guid id, string roomName, int capacity, string location, bool isClassroom)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return;

            room.RoomName = roomName;
            room.Capacity = capacity;
            room.Location = location;
            room.IsClassroom = isClassroom;
            room.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        // Soft delete room
        public async Task DeleteAsync(Guid id)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
            if (room == null) return;

            // Check if room is used in active course offerings
            var isUsed = await _context.CourseOfferings
                .AnyAsync(co => co.RoomId == id && !co.IsDeleted);

            if (isUsed)
                throw new InvalidOperationException(
                    "This room cannot be deleted because it is assigned to active course offerings."
                );

            room.IsDeleted = true;
            room.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        // Reactivate room
        public async Task ActivateAsync(Guid id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return;

            room.IsDeleted = false;
            room.DeletedAt = null;
            await _context.SaveChangesAsync();
        }

        // Get classroom status
        public async Task<List<ClassroomStatusDto>> GetClassroomStatusAsync()
        {
            var now = DateTime.Now;
            var currentDayOfWeek = now.DayOfWeek;
            var currentTime = TimeOnly.FromDateTime(now);

            // Fetch only classrooms
            var classrooms = await _context.Rooms
                .Where(r => !r.IsDeleted && r.IsClassroom)
                .ToListAsync();

            // Fetch ongoing offerings today
            var offeringsTodayQuery =
                from co in _context.CourseOfferings
                join c in _context.Courses on co.CourseId equals c.Id
                where !co.IsDeleted &&
                      (
                        (currentDayOfWeek == DayOfWeek.Monday && co.Mon) ||
                        (currentDayOfWeek == DayOfWeek.Tuesday && co.Tue) ||
                        (currentDayOfWeek == DayOfWeek.Wednesday && co.Wed) ||
                        (currentDayOfWeek == DayOfWeek.Thursday && co.Thu) ||
                        (currentDayOfWeek == DayOfWeek.Friday && co.Fri) ||
                        (currentDayOfWeek == DayOfWeek.Saturday && co.Sat) ||
                        (currentDayOfWeek == DayOfWeek.Sunday && co.Sun)
                      ) &&
                      co.StartTime <= currentTime && co.EndTime >= currentTime
                select new
                {
                    co.RoomId,
                    c.Code,
                    c.Title,
                    co.StartTime,
                    co.EndTime
                };

            var offeringsToday = await offeringsTodayQuery.ToListAsync();

            // Map classrooms with ongoing schedules
            var result = classrooms.Select(r =>
            {
                var currentSchedules = offeringsToday
                    .Where(o => o.RoomId == r.Id)
                    .Select(o => new ScheduleDto
                    {
                        CourseCode = o.Code,
                        CourseName = o.Title,
                        StartTime = o.StartTime,
                        EndTime = o.EndTime
                    })
                    .ToList();

                return new ClassroomStatusDto
                {
                    Id = r.Id,
                    RoomName = r.RoomName,
                    Location = r.Location,
                    Capacity = r.Capacity,
                    IsOccupied = currentSchedules.Any(),
                    CurrentSchedules = currentSchedules
                };
            }).OrderBy(x => x.RoomName).ToList();

            return result;
        }
    }
}
