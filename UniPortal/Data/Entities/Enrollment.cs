namespace UniPortal.Data.Entities
{
    public class Enrollment : IEntity
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid SemesterId { get; set; }
    }
}
