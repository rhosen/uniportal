using System.ComponentModel.DataAnnotations.Schema;

namespace UniPortal.Data.Entities
{
    public class Notification : IEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Guid NotificationTypeId { get; set; }
        public string ReceiverId { get; set; }

        // Navigation

        [ForeignKey(nameof(CreatedById))]
        public Account CreatedByAccount { get; set; }
        public NotificationType NotificationType { get; set; }

    }
}