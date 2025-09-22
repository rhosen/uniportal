using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;
using UniPortal.ViewModels.Academics;

namespace UniPortal.Services.Academics.Configs
{
    public class RoomService
    {
        private readonly UniPortalContext _context;

        public RoomService(UniPortalContext context)
        {
            _context = context;
        }

        public async Task<List<Room>> GetAllAsync()
        {
            return await _context.Rooms.Where(c => !c.IsDeleted)
                .OrderBy(c => c.RoomName)
                .ToListAsync();
        }

        public async Task<List<SelectOption>> GetOptionsAsync()
        {
            return await _context.Rooms
                .Where(r => !r.IsDeleted)
                .OrderBy(x=> x.RoomName)
                .Select(r => new SelectOption
                {
                    Id = r.Id,
                    Name = r.RoomName
                })
                .ToListAsync();
        }

        public async Task<Room> GetByIdAsync(string id)
        {
            return await _context.Rooms
                .FirstOrDefaultAsync(c => c.Id.ToString() == id);
        }

        public async Task CreateAsync(string roomName, int capacity, string location)
        {
            var classroom = new Room
            {
                RoomName = roomName,
                Capacity = capacity,
                Location = location
            };
            _context.Rooms.Add(classroom);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, string roomName, int capacity, string location)
        {
            var classroom = await _context.Rooms.FindAsync(id);
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
            if (!Guid.TryParse(id, out var roomId))
                return;

            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == roomId);
            if (room == null) return;

            // Check if room is used in any active course offerings
            var isUsed = await _context.CourseOfferings
                .AnyAsync(co => co.RoomId == roomId && !co.IsDeleted);

            if (isUsed)
            {
                throw new InvalidOperationException(
                    "This room cannot be deleted because it is assigned to active course offerings."
                );
            }

            // Safe to soft delete
            room.IsDeleted = true;
            room.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }


        public async Task ActivateAsync(string id)
        {
            var classroom = await _context.Rooms.FindAsync(Guid.Parse(id));
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
            var currentDayOfWeek = now.DayOfWeek;
            var currentTime = TimeOnly.FromDateTime(now);

            // Fetch all classrooms
            var classrooms = await _context.Rooms
                .Where(r => !r.IsDeleted)
                .ToListAsync();

            // Fetch ongoing offerings today (join CourseOfferings + Courses)
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

            // Map classrooms with their ongoing schedules
            var result = classrooms.Select(r =>
            {
                var currentSchedules = offeringsToday
                    .Where(o => o.RoomId == r.Id)
                    .Select(o => new ScheduleDto
                    {
                        SubjectCode = o.Code,
                        SubjectName = o.Title,
                        StartTime = o.StartTime,
                        EndTime = o.EndTime
                    })
                    .ToList();

                return new ClassroomAvailabilityViewModel
                {
                    Id = r.Id,
                    RoomName = r.RoomName,
                    Location = r.Location,
                    Capacity = r.Capacity,
                    IsOccupied = currentSchedules.Any(),
                    CurrentSchedules = currentSchedules
                };
            }).ToList();

            return result;
        }


    }
}
