namespace UniPortal.Data.Entities
{
    public class CanceledClass : IEntity
    {
        public Guid ClassScheduleEntryId { get; set; } 
        public DateOnly Date { get; set; }
        public string Reason { get; set; }

        // Navigation property
        public ClassScheduleEntry ClassScheduleEntry { get; set; }
    }
}
