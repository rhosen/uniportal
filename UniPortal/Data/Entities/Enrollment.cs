namespace UniPortal.Data.Entities
{
    public class Enrollment : IEntity
    {
        public Guid StudentId { get; set; }
        public Guid CourseOfferingId { get; set; }   // FK

        // Navigation properties
        public Student Student { get; set; } = null!;
        public CourseOffering CourseOffering { get; set; } = null!;
    }
}
