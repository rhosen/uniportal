namespace UniPortal.Data.Entities
{
    public class CanceledClass : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ClassScheduleId { get; set; }   // Reference to the scheduled class
        public DateOnly Date { get; set; }          // The specific date the class is canceled
        public string Reason { get; set; }          // Optional reason

        // Navigation property
        public ClassSchedule ClassSchedule { get; set; }
    }
}
