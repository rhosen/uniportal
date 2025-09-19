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

        // Identity-linked entities
        public DbSet<Account> Accounts { get; set; }

        // Academic structure
        public DbSet<Department> Departments { get; set; }
        public DbSet<Entities.Program> Programs { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Room> Rooms { get; set; }

        // Students
        public DbSet<Student> Students { get; set; }

        // Student-related entities
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<GradeScale> GradeScales { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Entities.File> Files { get; set; }
        public DbSet<Notice> Notices { get; set; }
        public DbSet<Recipient> Recipients { get; set; }

        // Scheduling / system
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Log> Logs { get; set; }
    }
}
