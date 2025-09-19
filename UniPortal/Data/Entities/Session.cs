namespace UniPortal.Data.Entities
{
    public class Session : IEntity
    {
        public Guid ScheduleId { get; set; }
        public int DayOfWeek { get; set; }   // 1=Monday, 7=Sunday
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        // Navigation properties
        public Schedule Schedule { get; set; }
    }
}
