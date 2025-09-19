using System.ComponentModel.DataAnnotations.Schema;

namespace UniPortal.Data.Entities
{
    public class Notice : IEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Guid RecipientId { get; set; }
        public string StudentId { get; set; }

        // Navigation

        [ForeignKey(nameof(ModifiedById))]
        public Account Sender { get; set; }
        public Recipient Recipient { get; set; }

    }
}