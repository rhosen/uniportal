namespace UniPortal.Data.Entities
{
    public class Cancellation : IEntity
    {
        public Guid SessionId { get; set; } 
        public DateOnly Date { get; set; }
        public string Reason { get; set; }

        // Navigation property
        public Session Session { get; set; }
    }
}
