namespace UniPortal.Data.Entities
{
    public class Attendance: IEntity
    {
        public Guid StudentId { get; set; }
        public Guid ClassScheduleId { get; set; }
        public string Status { get; set; } = "Absent"; // Present, Absent, Late
        public DateTime RecordedAt { get; set; } = DateTime.Now;
    }
}
