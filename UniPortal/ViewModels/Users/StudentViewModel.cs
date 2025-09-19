namespace UniPortal.ViewModels.Users
{
    public class StudentViewModel
    {
        // Identifiers
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public string StudentId { get; set; }

        // Academic Info
        public string BatchNumber { get; set; }
        public string Section { get; set; }
        public Guid ProgramId { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid? CurrentSemesterId { get; set; }

        // Contact Info
        public string Email { get; set; }

        // Display Names (computed/mapped)
        public string ProgramName { get; set; }
        public string DepartmentCode { get; set; }
        public string CurrentSemesterName { get; set; }
    }
}
