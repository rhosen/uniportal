namespace UniPortal.Data.Entities
{
    public class Semester : IEntity
    {
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        // Navigation
        public ICollection<Course> Courses { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
        public ICollection<Grade> Grades { get; set; }
        public ICollection<ClassSchedule> ClassSchedules { get; set; }
    }
}
