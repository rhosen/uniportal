using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;
using UniPortal.Data.Entities.UniPortal.Data.Entities;

namespace UniPortal.Data
{
    public class UniPortalContext : IdentityDbContext<IdentityUser>
    {
        public UniPortalContext(DbContextOptions<UniPortalContext> options)
            : base(options)
        {
        }

        // Accounts linked to IdentityUser
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Student> Students { get; set; }

        // Academic entities
        public DbSet<Department> Departments { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<ClassNote> ClassNotes { get; set; }
        public DbSet<ClassSchedule> ClassSchedules { get; set; }
        public DbSet<ClassScheduleEntry> ClassScheduleEntries { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }

        // Student-related entities
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Notice> Notices { get; set; }
        public DbSet<RecipientType> RecipientTypes { get; set; }

        // System entities
        public DbSet<Log> Logs { get; set; }
    }
}
