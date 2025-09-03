namespace UniPortal.Data.Entities
{
    public class ClassNote : IEntity
    {
        public Guid CourseId { get; set; }
        public Guid TeacherId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
