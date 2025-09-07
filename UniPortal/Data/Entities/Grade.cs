namespace UniPortal.Data.Entities
{
    public class Grade : IEntity
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid SemesterId { get; set; }
        public string? GradeValue { get; set; }
        public decimal? Marks { get; set; }


        // Navigation
        public Student Student { get; set; }
        public Course Course { get; set; }
        public Semester Semester { get; set; }
    }
}
