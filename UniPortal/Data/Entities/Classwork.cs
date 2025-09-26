namespace UniPortal.Data.Entities
{
    public class Classwork : IEntity
    {
        public Guid CourseOfferingId { get; set; }
        public Guid FacultyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public bool RequiresSubmission { get; set; }
        public DateTime? DueDate { get; set; }

    }
}
