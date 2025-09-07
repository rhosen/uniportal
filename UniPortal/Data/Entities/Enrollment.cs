namespace UniPortal.Data.Entities
{
    public class Enrollment : IEntity
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid SemesterId { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;
        // Navigation
        public Student Student { get; set; }
        public Course Course { get; set; }
        public Semester Semester { get; set; }
    }
}
