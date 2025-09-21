namespace UniPortal.Data.Entities
{
    public class Semester : IEntity
    {
        public string SemesterType { get; set; } = null!;   // 'Fall', 'Spring', 'Summer'
        public string AcademicYear { get; set; } = null!;   // e.g., "2025-2026"
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCurrent { get; set; } = false;
    }
}
