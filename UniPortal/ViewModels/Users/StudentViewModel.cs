namespace UniPortal.ViewModels.Users
{
    public class StudentViewModel
    {
        // Identifiers
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public string StudentId { get; set; } = string.Empty;

        // Academic Info
        public Guid BatchId { get; set; }          // Use ID for dropdown binding
        public Guid SectionId { get; set; }        // Use ID for dropdown binding
        public Guid ProgramId { get; set; }
        public Guid DepartmentId { get; set; }
        public int CurrentSemester { get; set; }

        // Contact Info
        public string Email { get; set; } = string.Empty;

        // Display Names (computed/mapped)
        public string BatchNumber { get; set; } = string.Empty;  // for display only
        public string Section { get; set; } = string.Empty;      // for display only
        public string ProgramName { get; set; } = string.Empty;
        public string DepartmentCode { get; set; } = string.Empty;
        public string CurrentSemesterName { get; set; } = string.Empty;
    }
}
