namespace UniPortal.Data.Entities
{
    public class Program : IEntity
    {
        public Guid DepartmentId { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public Guid DegreeId { get; set; }
        public int TotalSemesters { get; set; }
        public int TotalCreditsRequired { get; set; }
        public int Duration { get; set; }
        public virtual Department Department { get; set; } = null!;
        public virtual Degree Degree { get; set; } = null!;
    }
}
