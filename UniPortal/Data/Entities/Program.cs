namespace UniPortal.Data.Entities
{
    public class Program: IEntity
    {
        public Guid DepartmentId { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;


        // Navigation properties
        public virtual Department Department { get; set; } = null!;
        public virtual Account? ModifiedBy { get; set; }

        public virtual ICollection<Semester> Semesters { get; set; } = new List<Semester>();
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
