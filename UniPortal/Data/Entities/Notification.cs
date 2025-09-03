namespace UniPortal.Data.Entities
{
    public class Notification : IEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Guid CreatedBy { get; set; }
        public int NotificationTypeId { get; set; }
        public Guid? TargetId { get; set; }

    }
}