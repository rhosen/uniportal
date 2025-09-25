namespace UniPortal.Data.Entities
{
    public class RecipientType : IEntity
    {
        public string Name { get; set; } = string.Empty;       // e.g., "Student", "Department", "All"
        public string? Description { get; set; }               // nullable

        // Navigation
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
