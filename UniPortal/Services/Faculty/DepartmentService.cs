using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;

namespace UniPortal.Services
{
    public class DepartmentService
    {
        private readonly UniPortalContext _context;

        public DepartmentService(UniPortalContext context)
        {
            _context = context;
        }

        public async Task<List<Department>> GetAllAsync()
        {
            return await _context.Departments
                .Include(d => d.Head)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<Department> GetByIdAsync(string id)
        {
            return await _context.Departments
                .Include(d => d.Head)
                .FirstOrDefaultAsync(d => d.Id.ToString() == id);
        }

        public async Task CreateAsync(string name, string description, Guid? headId)
        {
            var dept = new Department
            {
                Name = name,
                Description = description,
                HeadId = headId
            };
            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, string name, string description, Guid? headId)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept != null)
            {
                dept.Name = name;
                dept.Description = description;
                dept.HeadId = headId;
                dept.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(string id)
        {
            var dept = await _context.Departments.FindAsync(Guid.Parse(id));
            if (dept != null)
            {
                dept.IsDeleted = true;
                dept.DeletedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ActivateAsync(string id)
        {
            var dept = await _context.Departments.FindAsync(Guid.Parse(id));
            if (dept != null)
            {
                dept.IsDeleted = false;
                dept.DeletedAt = null;
                await _context.SaveChangesAsync();
            }
        }
    }
}
