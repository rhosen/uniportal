namespace UniPortal.Data.Entities
{
    public class Attendance : IEntity
    {
        public Guid StudentId { get; set; }
        public Guid CourseOfferingId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; } = "Absent"; // Present, Absent, Late
    }
}
