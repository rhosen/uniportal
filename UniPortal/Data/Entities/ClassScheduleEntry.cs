namespace UniPortal.Data.Entities
{
    public class ClassScheduleEntry : IEntity
    {
        public Guid ScheduleId { get; set; }
        public int DayOfWeek { get; set; }   // 1=Monday, 7=Sunday
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        // Navigation properties
        public ClassSchedule Schedule { get; set; }
        public Account CreatedBy { get; set; }

        public ICollection<CanceledClass> CanceledClasses { get; set; } = new List<CanceledClass>();
    }
}
