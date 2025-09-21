namespace UniPortal.Data.Entities
{
    public class Faculty : IEntity
    {
        public Guid AccountId { get; set; }
        public Guid DepartmentId { get; set; }
        public string FacultyNumber { get; set; } = null!; // unique, not null

        // Navigation properties
        public Account Account { get; set; } = null!;
    }
}
