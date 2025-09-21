using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;

namespace UniPortal.Services.Academics.Configs
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
            return await _context.Departments.Where(x=> !x.IsDeleted)
                .OrderBy(d => d.Code)
                .ToListAsync();
        }

        public async Task<List<SelectOption>> GetDepartmentOptionsAsync()
        {
            return await _context.Departments
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .Select(d => new SelectOption
                {
                    Id = d.Id,
                    Name = d.Name
                })
                .ToListAsync();
        }


        public async Task<Department> GetByIdAsync(string id)
        {
            return await _context.Departments
                .FirstOrDefaultAsync(d => d.Id.ToString() == id);
        }

        public async Task CreateAsync(string name, string description)
        {
            var dept = new Department
            {
                Code = name,
                Name = description,
            };
            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, string name, string description)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept != null)
            {
                dept.Code = name;
                dept.Name = description;
                dept.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(string id)
        {
            var deptId = Guid.Parse(id);

            // Check if department exists
            var dept = await _context.Departments.FindAsync(deptId);
            if (dept == null) return;

            // Check if department has any related data
            var programIds = await _context.Programs
                                           .Where(p => p.DepartmentId == deptId && !p.IsDeleted)
                                           .Select(p => p.Id)
                                           .ToListAsync();

            var hasRelatedData = programIds.Any() ||
                                 await _context.Curriculums.AnyAsync(c => programIds.Contains(c.ProgramId) && !c.IsDeleted) ||
                                 await _context.CourseOfferings.AnyAsync(co => programIds.Contains(co.ProgramId) && co.FacultyId != Guid.Empty);

            if (hasRelatedData)
            {
                throw new InvalidOperationException(
                    "This department cannot be deleted because it contains related programs, courses, or faculty assignments.");
            }

            // Safe to delete
            dept.IsDeleted = true;
            dept.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
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
