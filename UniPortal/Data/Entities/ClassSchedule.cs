namespace UniPortal.Data.Entities
{
    public class ClassSchedule : IEntity
    {
        public Guid CourseId { get; set; }
        public Guid ClassroomId { get; set; }

        // Navigation
        public Course Course { get; set; }
        public Classroom Classroom { get; set; }
        public ICollection<Attendance> Attendances { get; set; }
        public ICollection<ClassScheduleEntry> Entries { get; set; } = new List<ClassScheduleEntry>();
    }
}
