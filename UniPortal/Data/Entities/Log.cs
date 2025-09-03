namespace UniPortal.Data.Entities
{
    public class Log
    {
        public Guid Id { get; set; }
        public Guid? AccountId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Entity { get; set; }
        public Guid? EntityId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
