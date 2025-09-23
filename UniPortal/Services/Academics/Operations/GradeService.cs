using Microsoft.EntityFrameworkCore;
using UniPortal.Data;
using UniPortal.Dtos;
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

        public async Task<List<SelectOption>> GetStudentsBySemesterAndTeacherAsync(int semesterNumber, Guid facultyId)
        {
            var students = await (
                from e in _context.Enrollments
                join co in _context.CourseOfferings on e.CourseOfferingId equals co.Id
                join s in _context.Students on e.StudentId equals s.Id
                join a in _context.Accounts on s.AccountId equals a.Id
                where co.SemesterNumber == semesterNumber
                      && co.FacultyId == facultyId
                      && !e.IsDeleted
                      && !s.IsDeleted
                select new
                {
                    s.Id,
                    s.StudentNumber,
                    a.FirstName,
                    a.LastName
                })
                .Distinct()
                .OrderBy(s => s.StudentNumber)
                .Select(s => new SelectOption
                {
                    Id = s.Id,
                    Name = $"{s.StudentNumber} - {s.FirstName} {s.LastName}"
                })
                .ToListAsync();

            return students;
        }



        // -------------------------
        // Student perspective
        // -------------------------
        public async Task<List<StudenGradeDto>> GetGradesForStudentAsync(Guid accountId)
        {
            var studentId = await _context.Students
                .Where(s => s.AccountId == accountId)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            if (studentId == Guid.Empty)
                return new List<StudenGradeDto>();

            // Only include courses where student is enrolled
            var query = from g in _context.Grades
                        join co in _context.CourseOfferings on g.CourseOfferingId equals co.Id
                        join e in _context.Enrollments on new { g.StudentId, g.CourseOfferingId }
                                                          equals new { e.StudentId, e.CourseOfferingId }
                        join c in _context.Courses on co.CourseId equals c.Id
                        join t in _context.Accounts on co.FacultyId equals t.Id
                        where g.StudentId == studentId
                              && !g.IsDeleted
                              && !co.IsDeleted
                              && !c.IsDeleted
                              && !e.IsDeleted
                        orderby co.SemesterNumber
                        select new StudenGradeDto
                        {
                            SemesterName = "Semester " + co.SemesterNumber,
                            SubjectCode = c.Code,
                            SubjectName = c.Title,
                            TeacherName = t.FirstName + " " + t.LastName,
                            Grade = g.GradeValue,
                            Marks = g.Marks,
                            GPA = g.GPA,
                            IsFail = g.GradeValue.Trim().ToUpper() == "F"
                        };

            return await query.ToListAsync();
        }

        // -------------------------
        // Teacher perspective
        // -------------------------
        public async Task<List<TeacherGradeViewModel>> GetGradesForTeacherAsync(
            Guid facultyId, int semesterNumber, Guid studentId)
        {
            var query = from e in _context.Enrollments
                        join co in _context.CourseOfferings on e.CourseOfferingId equals co.Id
                        join c in _context.Courses on co.CourseId equals c.Id
                        join s in _context.Students on e.StudentId equals s.Id
                        join a in _context.Accounts on s.AccountId equals a.Id
                        join t in _context.Accounts on co.FacultyId equals t.Id
                        join g in _context.Grades
                            on new { e.StudentId, e.CourseOfferingId } equals new { g.StudentId, g.CourseOfferingId } into gj
                        from grade in gj.DefaultIfEmpty()
                        where co.FacultyId == facultyId
                              && co.SemesterNumber == semesterNumber
                              && e.StudentId == studentId
                              && !e.IsDeleted
                              && !co.IsDeleted
                              && !c.IsDeleted
                              && !s.IsDeleted
                        select new TeacherGradeViewModel
                        {
                            StudentId = s.Id,
                            StudentName = a.FirstName + " " + a.LastName,
                            CourseOfferingId = co.Id,
                            SubjectCode = c.Code,
                            SubjectName = c.Title,
                            TeacherName = t.FirstName + " " + t.LastName,
                            Grade = grade != null ? grade.GradeValue : null,
                            Marks = grade != null ? grade.Marks : (decimal?)null,
                            GPA = grade != null ? grade.GPA : (decimal?)null
                        };

            return await query
                .OrderBy(x => x.StudentName)
                .ThenBy(x => x.SubjectCode)
                .ToListAsync();
        }

        // -------------------------
        // Upsert grade
        // -------------------------
        public async Task UpsertGradeAsync(Guid studentId, Guid courseOfferingId, decimal marks, Guid modifiedBy)
        {
            // Ensure student is enrolled
            var enrolled = await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.CourseOfferingId == courseOfferingId && !e.IsDeleted);

            if (!enrolled)
                throw new InvalidOperationException("Student is not enrolled in this course offering.");

            // 1️⃣ Get grade from GradeScale
            var gradeScale = await _context.GradeScales
                .FirstOrDefaultAsync(g => marks >= g.MinMarks && marks <= g.MaxMarks);

            if (gradeScale == null)
                throw new InvalidOperationException("No grade scale configured for these marks.");

            // 2️⃣ Upsert grade
            var existing = await _context.Grades
                .FirstOrDefaultAsync(g => g.StudentId == studentId && g.CourseOfferingId == courseOfferingId);

            if (existing != null)
            {
                existing.GradeValue = gradeScale.Grade;
                existing.GPA = gradeScale.GPA;
                existing.Marks = marks;
                existing.ModifiedById = modifiedBy;
                existing.UpdatedAt = DateTime.Now;
            }
            else
            {
                _context.Grades.Add(new Data.Entities.Grade
                {
                    StudentId = studentId,
                    CourseOfferingId = courseOfferingId,
                    GradeValue = gradeScale.Grade,
                    GPA = gradeScale.GPA,
                    Marks = marks,
                    ModifiedById = modifiedBy,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
