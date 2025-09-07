namespace UniPortal.ViewModel
{
    public class StudentProfileViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty; // human-readable
        public string Department { get; set; } = string.Empty;
        public string Batch { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}
