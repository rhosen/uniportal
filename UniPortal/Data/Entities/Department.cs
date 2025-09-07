namespace UniPortal.Data.Entities
{
    public class Department : IEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; }
        public Guid? HeadId { get; set; }

        public Account Head { get; set; }
        public ICollection<Course> Courses { get; set; }
    }

}
