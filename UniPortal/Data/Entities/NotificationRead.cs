namespace UniPortal.Data.Entities
{
    public class NotificationRead
    {
        public Guid Id { get; set; } 
        public Guid NotificationId { get; set; }
        public Guid AccountId { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
    }
}
