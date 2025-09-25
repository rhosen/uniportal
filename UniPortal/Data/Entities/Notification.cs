namespace UniPortal.Data.Entities
{
    public class Notification : IEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Guid RecipientTypeId { get; set; }
        public Guid? AccountId { get; set; }
        public string? FilePath { get; set; }     
        public RecipientType RecipientType { get; set; } = null!;
    }
}
