namespace UniPortal.Data.Entities
{
    public class Section : IEntity
    {
        public string Name { get; set; } = null!;   // e.g., "A", "B", "C"

        // Navigation properties
        public ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();
    }
}
