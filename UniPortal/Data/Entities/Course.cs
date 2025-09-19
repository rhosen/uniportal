namespace UniPortal.Data.Entities
{
    public class Course : IEntity
    {
        public Guid SubjectId { get; set; } 
        public Guid DepartmentId { get; set; }
        public Guid TeacherId { get; set; }
        public Guid SemesterId { get; set; }
        public int Credits { get; set; } = 3;


        // Navigation
        public Subject Subject { get; set; }
        public Department Department { get; set; }
        public Account Teacher { get; set; }
        public Semester Semester { get; set; }
        public ICollection<Note> Notes { get; set; }
        public ICollection<Schedule> Schedules { get; set; }
        public ICollection<Assignment> Assignments { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
        public ICollection<Grade> Grades { get; set; }
    }

}
