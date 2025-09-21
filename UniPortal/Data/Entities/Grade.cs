namespace UniPortal.Data.Entities
{
    public class Grade : IEntity
    {
        public Guid StudentId { get; set; }
        public Guid CourseOfferingId { get; set; }  // corrected FK

        public string GradeValue { get; set; } = string.Empty;
        public decimal Marks { get; set; }
        public decimal GPA { get; set; }

        // Navigation properties
        public Student Student { get; set; } = null!;
        public CourseOffering CourseOffering { get; set; } = null!;
    }
}
