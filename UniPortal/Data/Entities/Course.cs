namespace UniPortal.Data.Entities
{
    public class Course : IEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int CreditHours { get; set; }
        public Guid DepartmentId { get; set; }

        // Navigation property
        public virtual Department Department { get; set; } = null!;
    }
}
