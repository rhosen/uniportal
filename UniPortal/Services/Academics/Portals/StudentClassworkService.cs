using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;
using UniPortal.Dtos.Classwork;
using UniPortal.Helpers;

namespace UniPortal.Services.Academics.Portals
{
    public class StudentClassworkService
    {
        private readonly UniPortalContext _context;

        public StudentClassworkService(UniPortalContext context)
        {
            _context = context;
        }

        // 1️⃣ Semester dropdown
        public async Task<List<SelectOption>> GetSemesterOptionsAsync()
        {
            return await _context.Semesters
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.AcademicYear)
                .ThenBy(s => s.SemesterType)
                .Select(s => new SelectOption
                {
                    Id = s.Id,
                    Name = s.IsCurrent
                        ? $"{s.SemesterType} {s.AcademicYear} (Current)"
                        : $"{s.SemesterType} {s.AcademicYear}"
                })
                .ToListAsync();
        }

        // 2️⃣ Course dropdown filtered by semester
        public async Task<List<SelectOption>> GetCourseOptionsAsync(Guid semesterId)
        {
            return await (
                 from co in _context.CourseOfferings
                 join c in _context.Courses on co.CourseId equals c.Id
                 where !c.IsDeleted && co.SemesterId == semesterId
                 orderby c.Title
                 select new SelectOption
                 {
                     Id = co.Id,
                     Name = c.Title
                 }
             ).ToListAsync();
        }

        // 3️⃣ Paginated classwork list with search
        public async Task<(List<StudentClassworkDto> Classworks, int TotalCount)> GetClassworkListAsync(
            Guid studentId,
            Guid? semesterId = null,
            Guid? courseOfferingId = null,
            string? searchTerm = null,
            int page = 1,
            int pageSize = 10)
        {
            var skip = (page - 1) * pageSize;

            var query = from cw in _context.Classworks
                        join co in _context.CourseOfferings on cw.CourseOfferingId equals co.Id
                        join s in _context.Semesters on co.SemesterId equals s.Id
                        where !string.IsNullOrEmpty(cw.Title)
                        select new { cw, co, s };

            if (semesterId.HasValue)
                query = query.Where(x => x.s.Id == semesterId.Value);

            if (courseOfferingId.HasValue)
                query = query.Where(x => x.co.Id == courseOfferingId.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(x =>
                    x.cw.Title.Contains(searchTerm) ||
                    x.cw.Description.Contains(searchTerm));

            var totalCount = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.cw.UploadedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(x => new StudentClassworkDto
                {
                    Id = x.cw.Id,
                    Title = x.cw.Title,
                    Description = x.cw.Description,
                    RequiresSubmission = x.cw.RequiresSubmission,
                    DueDate = x.cw.DueDate,
                    UploadedAt = !string.IsNullOrEmpty(x.cw.FilePath)
                                ? x.cw.UploadedAt
                                : x.cw.CreatedAt, // fallback
                    FilePath = x.cw.FilePath,
                    HasSubmitted = _context.ClassworkSubmissions
                        .Any(sub => sub.ClassworkId == x.cw.Id && sub.StudentId == studentId)
                })
                .ToListAsync();

            return (list, totalCount);
        }

        // 4️⃣ Get classwork details + submission info
        public async Task<StudentClassworkDetailsDto?> GetClassworkDetailsAsync(Guid classworkId, Guid studentId)
        {
            return await (from cw in _context.Classworks
                          where cw.Id == classworkId
                          select new StudentClassworkDetailsDto
                          {
                              Id = cw.Id,
                              Title = cw.Title,
                              Description = cw.Description,
                              RequiresSubmission = cw.RequiresSubmission,
                              DueDate = cw.DueDate,
                              FilePath = cw.FilePath,
                              UploadedAt = cw.UploadedAt,
                              HasSubmitted = _context.ClassworkSubmissions
                                  .Any(sub => sub.ClassworkId == cw.Id && sub.StudentId == studentId),
                              SubmittedFilePath = _context.ClassworkSubmissions
                                  .Where(sub => sub.ClassworkId == cw.Id && sub.StudentId == studentId)
                                  .Select(sub => sub.FilePath)
                                  .FirstOrDefault(),
                              Remarks = _context.ClassworkSubmissions
                                  .Where(sub => sub.ClassworkId == cw.Id && sub.StudentId == studentId)
                                  .Select(sub => sub.Remarks)
                                  .FirstOrDefault(),
                              SubmittedAt = _context.ClassworkSubmissions
                                  .Where(sub => sub.ClassworkId == cw.Id && sub.StudentId == studentId)
                                  .Select(sub => sub.UploadedAt)
                                  .FirstOrDefault()
                          }).FirstOrDefaultAsync();
        }

        // 5️⃣ Submit or update classwork
        public async Task SubmitClassworkAsync(
            Guid classworkId,
            Guid studentId,
            IFormFile file,
            string remarks,
            string courseOfferingId)
        {
            var cw = await _context.Classworks.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == classworkId);

            if (cw == null)
                throw new InvalidOperationException("Classwork not found.");

            if (!cw.RequiresSubmission)
                throw new InvalidOperationException("This classwork does not require submission.");

            if (cw.DueDate.HasValue && DateTime.Now > cw.DueDate.Value)
                throw new InvalidOperationException("Cannot submit after due date.");

            if (file == null || file.Length == 0)
                throw new InvalidOperationException("File is required for submission.");

            var filePath = await FileHelper.SaveFileAsync(file, UploadType.Submission, courseOfferingId);

            var submission = await _context.ClassworkSubmissions
                .FirstOrDefaultAsync(s => s.ClassworkId == classworkId && s.StudentId == studentId);

            if (submission != null)
            {
                // Update
                submission.FilePath = filePath;
                submission.Remarks = remarks;
                submission.UploadedAt = DateTime.Now;
            }
            else
            {
                // Insert
                _context.ClassworkSubmissions.Add(new ClassworkSubmission
                {
                    Id = Guid.NewGuid(),
                    ClassworkId = classworkId,
                    StudentId = studentId,
                    FilePath = filePath,
                    Remarks = remarks,
                    UploadedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
        }

        // 6️⃣ Delete submission (before due date only)
        public async Task DeleteSubmissionAsync(Guid classworkId, Guid studentId)
        {
            var cw = await _context.Classworks.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == classworkId);

            if (cw == null)
                throw new InvalidOperationException("Classwork not found.");

            if (cw.DueDate.HasValue && DateTime.Now > cw.DueDate.Value)
                throw new InvalidOperationException("Cannot delete submission after due date.");

            var submission = await _context.ClassworkSubmissions
                .FirstOrDefaultAsync(s => s.ClassworkId == classworkId && s.StudentId == studentId);

            if (submission == null)
                throw new InvalidOperationException("No submission found to delete.");

            _context.ClassworkSubmissions.Remove(submission);
            await _context.SaveChangesAsync();
        }

        // 7️⃣ Can student submit?
        public async Task<bool> CanSubmitAsync(Guid classworkId)
        {
            var cw = await _context.Classworks.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == classworkId);

            if (cw == null || !cw.RequiresSubmission)
                return false;

            if (cw.DueDate.HasValue && DateTime.Now > cw.DueDate.Value)
                return false;

            return true;
        }
    }
}
