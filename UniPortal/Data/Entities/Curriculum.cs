namespace UniPortal.Data.Entities
{
    public class Curriculum : IEntity
    {
        public Guid ProgramId { get; set; }
        public Guid SemesterId { get; set; }
        public int SemesterNumber { get; set; }         // Added
        public Guid CourseId { get; set; }
        public int CreditHours { get; set; }
        public Guid CourseTypeId { get; set; }          // Updated from RequirementTypeId
        public int SequenceOrder { get; set; } = 1;

        // Navigation properties
        public Program Program { get; set; } = null!;
        public Semester Semester { get; set; } = null!;
        public Course Course { get; set; } = null!;
        public CourseType CourseType { get; set; } = null!;
    }
}
