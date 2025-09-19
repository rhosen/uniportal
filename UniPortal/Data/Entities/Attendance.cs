namespace UniPortal.Data.Entities
{
    public class Attendance: IEntity
    {
        public Guid StudentId { get; set; }
        public Guid ScheduleId { get; set; }
        public string Status { get; set; } = "Absent"; // Present, Absent, Late

        // Navigation
        public Student Student { get; set; }
        public Schedule Schedule { get; set; }
    }
}
