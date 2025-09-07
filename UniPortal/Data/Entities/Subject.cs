namespace UniPortal.Data.Entities
{
    public class Subject: IEntity
    {
        public string Code { get; set; } = string.Empty; 
        public string Name { get; set; } = string.Empty; 

        // 🔗 Navigation properties
        public Account? CreatedBy { get; set; }
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
