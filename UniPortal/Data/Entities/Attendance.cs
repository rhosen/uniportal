namespace UniPortal.Data.Entities
{
    public class Attendance: IEntity
    {
        public Guid StudentId { get; set; }
        public Guid ClassScheduleId { get; set; }
        public string Status { get; set; } = "Absent"; // Present, Absent, Late

        // Navigation
        public Student Student { get; set; }
        public ClassSchedule ClassSchedule { get; set; }
    }
}
