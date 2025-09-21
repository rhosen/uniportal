using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;

namespace UniPortal.Services.Academics.Configs
{
    public class ProgramService
    {
        private readonly UniPortalContext _context;

        public ProgramService(UniPortalContext context)
        {
            _context = context;
        }

        // Get select options for active programs with non-deleted departments
        public async Task<List<SelectOption>> GetSelectOptionsAsync()
        {
            return await _context.Programs
                .Where(p => !p.IsDeleted && !p.Department.IsDeleted)
                .OrderBy(p => p.Name)
                .Select(p => new SelectOption
                {
                    Id = p.Id,
                    Name = p.Name
                })
                .ToListAsync();
        }

        // Get all programs (include Department and Degree) with non-deleted departments
        public async Task<List<Data.Entities.Program>> GetAllAsync()
        {
            return await _context.Programs
                .Include(p => p.Department)
                .Include(p => p.Degree)
                .Where(p => !p.IsDeleted && !p.Department.IsDeleted)
                .OrderBy(p => p.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        // Get program by Id (only if department is not deleted)
        public async Task<Data.Entities.Program?> GetByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out var guid)) return null;

            return await _context.Programs
                .Include(p => p.Department)
                .Include(p => p.Degree)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == guid && !p.Department.IsDeleted);
        }

        // Create program
        public async Task CreateAsync(
            string code,
            string name,
            Guid departmentId,
            Guid degreeId,
            int totalSemesters,
            int totalCreditsRequired,
            bool isActive = true)
        {
            var program = new Data.Entities.Program
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = name,
                DepartmentId = departmentId,
                DegreeId = degreeId,
                TotalSemesters = totalSemesters,
                TotalCreditsRequired = totalCreditsRequired,
                CreatedAt = DateTime.Now
            };

            _context.Programs.Add(program);
            await _context.SaveChangesAsync();
        }

        // Update program
        public async Task UpdateAsync(
            Guid id,
            string code,
            string name,
            Guid departmentId,
            Guid degreeId,
            int totalSemesters,
            int totalCreditsRequired)
        {
            var program = await _context.Programs.FirstOrDefaultAsync(p => p.Id == id);
            if (program != null)
            {
                program.Code = code;
                program.Name = name;
                program.DepartmentId = departmentId;
                program.DegreeId = degreeId;
                program.TotalSemesters = totalSemesters;
                program.TotalCreditsRequired = totalCreditsRequired;
                program.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
            }
        }

        // Soft delete program
        public async Task DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out var programId))
                return;

            var program = await _context.Programs.FirstOrDefaultAsync(p => p.Id == programId);
            if (program == null) return;

            // 1️⃣ Check if program has any active curriculums
            var hasCurriculums = await _context.Curriculums
                .AnyAsync(c => c.ProgramId == programId && !c.IsDeleted);

            // 2️⃣ Check if program has any active course offerings
            var hasCourseOfferings = await _context.CourseOfferings
                .AnyAsync(co => co.ProgramId == programId && !co.IsDeleted);

            // 3️⃣ Check if program has any enrolled students
            var hasStudents = await _context.Students
                .AnyAsync(s => s.ProgramId == programId && !s.IsDeleted);

            // If any related data exists, prevent deletion
            if (hasCurriculums || hasCourseOfferings || hasStudents)
            {
                throw new InvalidOperationException(
                    "This program cannot be deleted because it contains related curriculums, courses, or enrolled students."
                );
            }

            // Safe to soft delete
            program.IsDeleted = true;
            program.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }


        // Activate (undo soft delete)
        public async Task ActivateAsync(string id)
        {
            if (!Guid.TryParse(id, out var guid)) return;

            var program = await _context.Programs.FirstOrDefaultAsync(p => p.Id == guid);
            if (program != null && program.IsDeleted)
            {
                program.IsDeleted = false;
                program.DeletedAt = null;
                await _context.SaveChangesAsync();
            }
        }

        // Get program options (only programs with non-deleted departments)
        public async Task<List<SelectOption>> GetProgramOptionsAsync()
        {
            return await _context.Programs
                .Where(p => !p.IsDeleted && !p.Department.IsDeleted)
                .Select(p => new SelectOption { Id = p.Id, Name = p.Name })
                .ToListAsync();
        }
    }
}
