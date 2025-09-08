using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;

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
            return await _context.Classrooms.Where(c=> !c.IsDeleted)
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
    }
}
