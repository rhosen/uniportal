namespace UniPortal.Data.Entities
{
    public class Schedule : IEntity
    {
        public Guid CourseId { get; set; }
        public Guid RoomId { get; set; }

        // Navigation
        public Course Course { get; set; }
        public Room Room { get; set; }
        public ICollection<Attendance> Attendances { get; set; }
        public ICollection<Session> Entries { get; set; } = new List<Session>();
    }
}
