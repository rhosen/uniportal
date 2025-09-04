namespace UniPortal.Data.Entities
{
    public class Department : IEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? HeadId { get; set; }

        public Account Head { get; set; }
        public ICollection<Course> Courses { get; set; }
    }

}
