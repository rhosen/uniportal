using System.ComponentModel.DataAnnotations.Schema;

namespace UniPortal.Data.Entities
{
    public class Notice : IEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Guid RecipientTypeId { get; set; }
        public string RecipientId { get; set; }

        // Navigation

        [ForeignKey(nameof(CreatedById))]
        public Account Sender { get; set; }
        public RecipientType RecipientType { get; set; }

    }
}