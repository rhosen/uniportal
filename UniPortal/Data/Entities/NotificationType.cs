namespace UniPortal.Data.Entities
{
    public class NotificationType : IEntity
    {
        public string Name { get; set; } = string.Empty;       // e.g., "Student", "Department", "All"
        public string? Description { get; set; }

        // Navigation
        public ICollection<Notification> Notifications { get; set; }
    }
}
