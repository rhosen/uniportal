namespace UniPortal.Data.Entities
{
    public class Semester : IEntity
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid ProgramId { get; set; }

        // Navigation
        public virtual Program Program { get; set; }
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
