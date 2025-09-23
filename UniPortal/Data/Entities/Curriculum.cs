namespace UniPortal.Data.Entities
{
    public class Curriculum : IEntity
    {
        public Guid ProgramId { get; set; }
        public int SemesterNumber { get; set; }   
        public Guid CourseId { get; set; }
        public int Sequence { get; set; } = 1;

        // Navigation properties
        public Program Program { get; set; } = null!;
        public Course Course { get; set; } = null!;
    }
}
