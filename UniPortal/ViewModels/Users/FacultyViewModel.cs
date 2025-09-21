namespace UniPortal.ViewModels.Users
{
    public class FacultyViewModel
    {
        public Guid AccountId { get; set; }        
        public string FacultyNumber { get; set; } = null!;
        public Guid DepartmentId { get; set; }

        // Account info (joined)
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;

        // Department info (joined)
        public string DepartmentName { get; set; } = null!;
    }
}
