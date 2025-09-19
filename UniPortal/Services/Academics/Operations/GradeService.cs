using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.ViewModels.Grades;

namespace UniPortal.Services.Academics.Operations
{
    public class GradeService
    {
        private readonly UniPortalContext _context;

        public GradeService(UniPortalContext context)
        {
            _context = context;
        }

        // -------------------------
        // Student perspective
        // -------------------------
        public async Task<List<GradeViewModel>> GetGradesForStudentAsync(Guid accountId)
        {
            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.AccountId == accountId);

            if (student == null)
                return new List<GradeViewModel>();

            return await _context.Grades
                .Where(g => g.StudentId == student.Id && !g.IsDeleted)
                .Include(g => g.Course)
                    .ThenInclude(c => c.Subject)
                .Include(g => g.Course)
                    .ThenInclude(c => c.Teacher)
                .Include(g => g.Course)
                    .ThenInclude(c => c.Semester)  // Include Semester
                .Select(g => new GradeViewModel
                {
                    SemesterName = g.Course.Semester != null
                        ? g.Course.Semester.Name
                        : "Unknown Semester",          // Avoid null keys
                    SubjectCode = g.Course.Subject.Code,
                    SubjectName = g.Course.Subject.Name,
                    TeacherName = g.Course.Teacher.FirstName + " " + g.Course.Teacher.LastName,
                    Grade = g.GradeValue,
                    Marks = g.Marks,
                    GPA = g.GPA,                 // Add GPA if available
                    IsFail = g.GradeValue.Trim().ToUpper() == "F"
                })
                .ToListAsync();
        }


        // -------------------------
        // Teacher perspective
        // -------------------------
        public async Task<List<TeacherGradeViewModel>> GetGradesForTeacherAsync(Guid teacherAccountId, Guid semesterId, Guid studentId)
        {
            var query = from e in _context.Enrollments.AsNoTracking()
                        join s in _context.Students on e.StudentId equals s.Id
                        join a in _context.Accounts on s.AccountId equals a.Id
                        join c in _context.Courses on e.CourseId equals c.Id
                        join sub in _context.Subjects on c.SubjectId equals sub.Id
                        join t in _context.Accounts on c.TeacherId equals t.Id
                        join g in _context.Grades
                            on new { e.StudentId, e.CourseId } equals new { g.StudentId, g.CourseId } into gj
                        from grade in gj.DefaultIfEmpty() // left join to get null if no grade
                        where c.TeacherId == teacherAccountId
                              && c.SemesterId == semesterId
                              && s.Id == studentId
                              && !s.IsDeleted
                              && !c.IsDeleted
                        select new TeacherGradeViewModel
                        {
                            StudentId = s.Id,
                            StudentName = a.FirstName + " " + a.LastName,
                            CourseId = c.Id,
                            SubjectId = sub.Id,
                            SubjectCode = sub.Code,
                            SubjectName = sub.Name,
                            TeacherName = t.FirstName + " " + t.LastName,
                            Grade = grade != null ? grade.GradeValue : null,
                            Marks = grade != null ? grade.Marks : (decimal?)null
                        };

            return await query
                .OrderBy(x => x.StudentName)
                .ThenBy(x => x.SubjectCode)
                .ToListAsync();
        }

        public async Task UpsertGradeAsync(Guid studentId, Guid courseId, decimal marks, Guid modifiedBy)
        {
            // 1. Get the appropriate grade from GradeScale based on marks
            var gradeScale = await _context.GradeScales
                .FirstOrDefaultAsync(g => marks >= g.MinMarks && marks <= g.MaxMarks);

            if (gradeScale == null)
                throw new InvalidOperationException("No grade scale configured for the given marks.");

            // 2. Check if a grade already exists
            var existing = await _context.Grades
                .FirstOrDefaultAsync(g => g.StudentId == studentId && g.CourseId == courseId);

            if (existing != null)
            {
                existing.GradeValue = gradeScale.Grade;
                existing.GPA = gradeScale.GPA;
                existing.Marks = marks;
                existing.ModifiedById = modifiedBy;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var grade = new Grade
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    GradeValue = gradeScale.Grade,
                    GPA = gradeScale.GPA,
                    Marks = marks,
                    ModifiedById = modifiedBy,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Grades.Add(grade);
            }

            await _context.SaveChangesAsync();
        }
    }
}
