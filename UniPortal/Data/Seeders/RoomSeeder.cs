using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public static class RoomSeeder
    {
        public static async Task SeedAsync(UniPortalContext dbContext)
        {
            if (await dbContext.Rooms.AnyAsync())
                return; // Already seeded

            var now = DateTime.Now;

            var rooms = new List<Room>();

            // Regular classrooms
            for (int i = 101; i <= 115; i++)
            {
                rooms.Add(new Room
                {
                    Id = Guid.NewGuid(),
                    RoomName = $"Room {i}",
                    Capacity = 25 + (i % 5) * 5,
                    Location = "Main Building",
                    IsClassroom = true,
                    CreatedAt = now,
                    IsDeleted = false
                });
            }

            // Labs
            for (int i = 201; i <= 210; i++)
            {
                rooms.Add(new Room
                {
                    Id = Guid.NewGuid(),
                    RoomName = $"Lab {i}",
                    Capacity = 20 + (i % 5) * 5,
                    Location = "Lab Building",
                    IsClassroom = true,
                    CreatedAt = now,
                    IsDeleted = false
                });
            }

            // Special rooms
            var specialRooms = new[] { "Conference Hall", "Auditorium", "Seminar Room", "Workshop Room" };
            rooms.AddRange(specialRooms.Select(r => new Room
            {
                Id = Guid.NewGuid(),
                RoomName = r,
                Capacity = r == "Auditorium" ? 100 : 60,
                Location = "Admin Block",
                IsClassroom = false,
                CreatedAt = now,
                IsDeleted = false
            }));

            dbContext.Rooms.AddRange(rooms);
            await dbContext.SaveChangesAsync();
        }
    }
}
