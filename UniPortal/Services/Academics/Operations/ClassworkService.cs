using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Dtos.Classwork;

namespace UniPortal.Services.Academics.Operations
{
    public class ClassworkService
    {
        private readonly UniPortalContext _context;

        public ClassworkService(UniPortalContext context)
        {
            _context = context;
        }

        // =========================
        // 1. Load courses for current faculty & semester (for header)
        // =========================
        public async Task<List<ClassworkDto>> GetFacultyCoursesAsync(Guid facultyId, Guid semesterId, Guid? batchId, Guid? sectionId)
        {
            var query = from co in _context.CourseOfferings
                        join c in _context.Courses on co.CourseId equals c.Id
                        join b in _context.Batches on co.BatchId equals b.Id
                        join s in _context.Sections on co.SectionId equals s.Id
                        join sem in _context.Semesters on co.SemesterId equals sem.Id
                        where co.FacultyId == facultyId && co.SemesterId == semesterId
                        && co.BatchId == batchId && co.SectionId == sectionId
                        && !co.IsDeleted
                        select new ClassworkDto
                        {
                            CourseOfferingId = co.Id,
                            CourseTitle = $"{c.Title}",
                            SemesterName = sem.IsCurrent
                                ? $"{sem.SemesterType} {sem.AcademicYear} (Current)"
                                : $"{sem.SemesterType} {sem.AcademicYear}",
                        };

            return await query.ToListAsync();
        }


        // =========================
        // 2. Left Sidebar: Classworks for course with search + pagination
        // =========================
        public async Task<(List<ClassworkDto> Items, int TotalCount)> GetClassworksAsync(
            Guid facultyId,
            Guid courseOfferingId,
            int page = 1,
            int pageSize = 10,
            string? searchTerm = null)
        {
            var query = _context.Classworks
                .Where(cw => cw.FacultyId == facultyId
                             && cw.CourseOfferingId == courseOfferingId
                             && !cw.IsDeleted);

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(cw => cw.Title.Contains(searchTerm));

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(cw => cw.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(cw => new ClassworkDto
                {
                    Id = cw.Id,
                    Title = cw.Title,
                    CourseOfferingId = cw.CourseOfferingId,
                    CreatedAt = cw.CreatedAt,
                    DueDate = cw.DueDate,
                    RequiresSubmission = cw.RequiresSubmission
                })
                .ToListAsync();

            return (items, total);
        }

        // =========================
        // 3. Middle panel: get single classwork details
        // =========================
        public async Task<ClassworkDto?> GetClassworkByIdAsync(Guid classworkId, Guid facultyId)
        {
            return await (from cw in _context.Classworks
                          join co in _context.CourseOfferings on cw.CourseOfferingId equals co.Id
                          join c in _context.Courses on co.CourseId equals c.Id
                          join s in _context.Sections on co.SectionId equals s.Id
                          join sem in _context.Semesters on co.SemesterId equals sem.Id
                          where cw.Id == classworkId && cw.FacultyId == facultyId && !cw.IsDeleted
                          select new ClassworkDto
                          {
                              Id = cw.Id,
                              Title = cw.Title,
                              Description = cw.Description,
                              FilePath = cw.FilePath,
                              RequiresSubmission = cw.RequiresSubmission,
                              DueDate = cw.DueDate,
                              CreatedAt = cw.CreatedAt,
                              CourseOfferingId = co.Id,
                              CourseTitle = c.Title,
                              SemesterName = $"{sem.SemesterType} {sem.AcademicYear}",
                          }).FirstOrDefaultAsync();
        }

        // =========================
        // 4. Submissions: Submitted + Not Submitted
        // =========================
        public async Task<(List<ClassworkSubmissionDto> Submitted, List<ClassworkSubmissionDto> NotSubmitted)>
            GetSubmissionsStatusAsync(Guid classworkId)
        {
            // submitted
            var submitted = await (from s in _context.ClassworkSubmissions
                                   join st in _context.Students on s.StudentId equals st.Id
                                   join a in _context.Accounts on st.AccountId equals a.Id
                                   where s.ClassworkId == classworkId && !s.IsDeleted
                                   select new ClassworkSubmissionDto
                                   {
                                       Id = s.Id,
                                       StudentId = st.Id,
                                       StudentName = a.FirstName + " " + a.LastName,
                                       FilePath = s.FilePath,
                                       Remarks = s.Remarks,
                                       CreatedAt = s.CreatedAt
                                   }).ToListAsync();

            // all students enrolled in this course
            var courseOfferingId = await _context.Classworks
                .Where(cw => cw.Id == classworkId)
                .Select(cw => cw.CourseOfferingId)
                .FirstAsync();

            var allStudents = await (from e in _context.Enrollments
                                     join st in _context.Students on e.StudentId equals st.Id
                                     join a in _context.Accounts on st.AccountId equals a.Id
                                     where e.CourseOfferingId == courseOfferingId && !e.IsDeleted
                                     select new ClassworkSubmissionDto
                                     {
                                         StudentId = st.Id,
                                         StudentName = a.FirstName + " " + a.LastName
                                     }).ToListAsync();

            var submittedIds = submitted.Select(s => s.StudentId).ToHashSet();
            var notSubmitted = allStudents.Where(st => !submittedIds.Contains(st.StudentId)).ToList();

            return (submitted, notSubmitted);
        }

        // =========================
        // 5. Create new classwork
        // =========================
        public async Task<Guid> CreateClassworkAsync(Guid facultyId, Guid courseOfferingId, ClassworkCreateUpdateDto dto)
        {
            if (facultyId == Guid.Empty)
                throw new InvalidOperationException("Invalid faculty id.");

            if (courseOfferingId == Guid.Empty)
                throw new InvalidOperationException("Invalid course offering id.");

            if (dto == null)
                throw new InvalidOperationException("Classwork data cannot be null.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new InvalidOperationException("Title is required.");

            if (dto.RequiresSubmission && !dto.DueDate.HasValue)
                throw new InvalidOperationException("Due date is required when submission is required.");

            if (!string.IsNullOrEmpty(dto.FilePath) && !Uri.IsWellFormedUriString(dto.FilePath, UriKind.RelativeOrAbsolute))
                throw new InvalidOperationException("Invalid file path.");

            // Create entity
            var cw = new Data.Entities.Classwork
            {
                Id = Guid.NewGuid(),
                FacultyId = facultyId,
                CourseOfferingId = courseOfferingId,
                Title = dto.Title,
                Description = dto.Description,
                FilePath = dto.FilePath,
                RequiresSubmission = dto.RequiresSubmission,
                DueDate = dto.DueDate,
                CreatedAt = DateTime.Now
            };

            _context.Classworks.Add(cw);
            await _context.SaveChangesAsync();

            return cw.Id;
        }


        // =========================
        // 6. Update classwork
        // =========================
        public async Task<bool> UpdateClassworkAsync(Guid classworkId, Guid facultyId, ClassworkCreateUpdateDto dto)
        {
            var cw = await _context.Classworks.FirstOrDefaultAsync(c => c.Id == classworkId && c.FacultyId == facultyId && !c.IsDeleted);
            if (cw == null) return false;

            var hasSubmission = await _context.ClassworkSubmissions.AnyAsync(s => s.ClassworkId == classworkId && !s.IsDeleted);
            if (hasSubmission)
                throw new InvalidOperationException("Cannot edit classwork because students have already submitted.");

            cw.Title = dto.Title;
            cw.Description = dto.Description;
            cw.FilePath = dto.FilePath;
            cw.RequiresSubmission = dto.RequiresSubmission;
            cw.DueDate = dto.DueDate;
            cw.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // =========================
        // 7. Delete classwork (soft delete)
        // =========================
        public async Task<bool> DeleteClassworkAsync(Guid classworkId)
        {
            var cw = await _context.Classworks.FirstOrDefaultAsync(c => c.Id == classworkId && !c.IsDeleted);
            if (cw == null) return false;

            var hasSubmission = await _context.ClassworkSubmissions.AnyAsync(s => s.ClassworkId == classworkId && !s.IsDeleted);
            if (hasSubmission)
                throw new InvalidOperationException("Cannot delete classwork because students have already submitted.");

            cw.IsDeleted = true;
            cw.DeletedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
