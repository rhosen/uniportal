namespace UniPortal.Data.Entities
{
    public class Attendance : IEntity
    {
        public Guid StudentId { get; set; }
        public Guid CourseOfferingId { get; set; }   // updated
        public string Status { get; set; } = "Absent"; // Present, Absent, Late
    }
}
