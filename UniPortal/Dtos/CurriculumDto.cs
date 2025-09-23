namespace UniPortal.Dtos
{
    public class CurriculumDto
    {
        public Guid Id { get; set; }
        public Guid ProgramId { get; set; }
        public string ProgramName { get; set; } = string.Empty;
        public Guid CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public bool IsDeleted { get; set; }
        public int SemesterNumber { get;  set; }
    }
}
