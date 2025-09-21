namespace UniPortal.Data.Entities
{
    public class CourseMaterial : IEntity
    {
        public Guid CourseOfferingId { get; set; }
        public Guid FacultyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string FilePath { get; set; } = string.Empty;  // store uploaded file path

        // Navigation properties
        public CourseOffering CourseOffering { get; set; } = null!;
        public Faculty Faculty { get; set; } = null!;
    }
}
