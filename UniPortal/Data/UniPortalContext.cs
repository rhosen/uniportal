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
        public DbSet<Degree> Degrees { get; set; }
        public DbSet<Entities.Program> Programs { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseOffering> CourseOfferings { get; set; }
        public DbSet<Room> Rooms { get; set; }

        // Curriculum / requirements
        public DbSet<CourseType> CourseTypes { get; set; }
        public DbSet<Curriculum> Curriculums { get; set; }

        // Students
        public DbSet<Batch> Batches { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Student> Students { get; set; }

        // Faculties / staff
        public DbSet<FacultyType> FacultyTypes { get; set; }
        public DbSet<Faculty> Faculties { get; set; }

        // Classwork & submissions
        public DbSet<Classwork> Classworks { get; set; }
        public DbSet<ClassworkSubmission> ClassworkSubmissions { get; set; }

        // Student-related entities
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<GradeScale> GradeScales { get; set; }
        public DbSet<Attendance> Attendances { get; set; }

        // Notifications
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationRead> NotificationReads { get; set; }
        public DbSet<RecipientType> RecipientTypes { get; set; }

        // Class cancellations
        public DbSet<ClassCancellation> ClassCancellations { get; set; }

        // System / logging
        public DbSet<Log> Logs { get; set; }
        public DbSet<Institution> Institutions { get; set; }
    }
}
