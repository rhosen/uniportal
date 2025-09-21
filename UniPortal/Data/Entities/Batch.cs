namespace UniPortal.Data.Entities
{
    public class Batch : IEntity
    {
        public string Number { get; set; } = null!;   // e.g., "2025", "2025-2026"
        public ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();
    }
}
