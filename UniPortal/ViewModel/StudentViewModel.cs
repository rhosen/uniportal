namespace UniPortal.ViewModel
{
    public class StudentViewModel
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public string StudentId { get; set; }
        public string BatchNumber { get; set; }
        public string Section { get; set; }
        public Guid? DepartmentId { get; set; }
        public string Email { get; set; }
    }
}
