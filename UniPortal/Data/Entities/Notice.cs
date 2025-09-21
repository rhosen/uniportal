using System.ComponentModel.DataAnnotations.Schema;

namespace UniPortal.Data.Entities
{
    public class Notice : IEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Guid RecipientId { get; set; }
        public string? TargetId { get; set; }
        public string? FilePath { get; set; }       // optional attachment path

        // Navigation properties
        [ForeignKey(nameof(ModifiedById))]
        public Account Sender { get; set; } = null!;
        public Recipient Recipient { get; set; } = null!;
    }
}
