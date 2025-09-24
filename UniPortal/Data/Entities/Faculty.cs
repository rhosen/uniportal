namespace UniPortal.Data.Entities
{
    public class Faculty : IEntity
    {
        public Guid AccountId { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid FacultyTypeId { get; set; }
        public string FacultyNumber { get; set; }
        public bool IsAdvisor { get; set; }
        public Account Account { get; set; }
    }
}
