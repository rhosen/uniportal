namespace UniPortal.Data.Entities
{
    public class Assignment : IEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid CourseOfferingId { get; set; }
        public DateTime AssignedDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; }
        public string? FilePath { get; set; }

        // Navigation
        public CourseOffering CourseOffering { get; set; }
        public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
    }
}
