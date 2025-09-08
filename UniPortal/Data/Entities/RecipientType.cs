namespace UniPortal.Data.Entities
{
    public class RecipientType : IEntity
    {
        public string Name { get; set; } = string.Empty;       // e.g., "Student", "Department", "All"
        public string Description { get; set; }

        // Navigation
        public ICollection<Notice> Notifications { get; set; }
    }
}
