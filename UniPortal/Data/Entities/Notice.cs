using System.ComponentModel.DataAnnotations.Schema;

namespace UniPortal.Data.Entities
{
    public class Notice : IEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Guid RecipientTypeId { get; set; }
        public string RecipientId { get; set; }
        public string? FilePath { get; set; }     
        public RecipientType RecipientType { get; set; } = null!;
    }
}
