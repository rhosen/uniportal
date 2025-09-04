using System.Diagnostics;

namespace UniPortal.Data.Entities
{
    public class Course : IEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public Guid TeacherId { get; set; }
        public Guid SemesterId { get; set; }
        public int Credits { get; set; } = 3;


        // Navigation
        public Department Department { get; set; }
        public Account Teacher { get; set; }
        public Semester Semester { get; set; }
        public ICollection<ClassNote> ClassNotes { get; set; }
        public ICollection<ClassSchedule> ClassSchedules { get; set; }
        public ICollection<Assignment> Assignments { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
        public ICollection<Grade> Grades { get; set; }
    }

}
