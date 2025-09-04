namespace UniPortal.Data.Entities
{
    public class ClassSchedule : IEntity
    {
        public Guid CourseId { get; set; }
        public Guid ClassroomId { get; set; }
        public Guid TeacherId { get; set; }
        public Guid SemesterId { get; set; }
        public int DayOfWeek { get; set; } // 1 = Monday, etc.
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }


        // Navigation
        public Course Course { get; set; }
        public Account Teacher { get; set; }
        public Classroom Classroom { get; set; }
        public Semester Semester { get; set; }
        public ICollection<Attendance> Attendances { get; set; }
    }
}
