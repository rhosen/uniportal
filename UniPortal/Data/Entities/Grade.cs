namespace UniPortal.Data.Entities
{
    public class Grade : IEntity
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid SemesterId { get; set; }
        public string? GradeValue { get; set; }
        public decimal? Marks { get; set; }
    }
}
