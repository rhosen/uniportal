namespace UniPortal.Dtos
{
    public class CurriculumDto
    {
        public Guid Id { get; set; }
        public Guid ProgramId { get; set; }
        public string ProgramName { get; set; } = string.Empty;
        public Guid SemesterId { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public Guid CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public Guid CourseTypeId { get; set; }
        public string CourseTypeName { get; set; } = string.Empty;
        public int SequenceOrder { get; set; }
        public bool IsDeleted { get; set; }
        public int SemesterNumber { get;  set; }
    }
}
