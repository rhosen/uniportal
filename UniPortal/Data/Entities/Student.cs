namespace UniPortal.Data.Entities
{
    public class Student : IEntity
    {
        public Guid AccountId { get; set; }
        public string StudentNumber { get; set; } = null!;
        public Guid BatchId { get; set; }
        public Guid SectionId { get; set; }
        public Guid ProgramId { get; set; }
        public int CurrentSemester { get; set; } = 1;
        public DateTime? GraduationDate { get; set; }
        public Account Account { get; set; } = null!;
    }
}
