namespace UniPortal.Data.Entities
{
    public class Assignment : IEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid CourseId { get; set; }
        public DateTime AssignedDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; }
    }
}
