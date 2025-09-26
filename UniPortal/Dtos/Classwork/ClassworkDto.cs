namespace UniPortal.Dtos.Classwork
{
    public class ClassworkDto
    {
        public Guid Id { get; set; }

        // Course info for sidebar/header
        public Guid CourseOfferingId { get; set; }
        public string CourseTitle { get; set; }
        public string SemesterName { get; set; }

        // Classwork info
        public string Title { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public bool RequiresSubmission { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
