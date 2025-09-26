namespace UniPortal.ViewModels.Operations
{
    public class AssignmentViewModel
    {
        public Guid Id { get; set; }             
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; 
        public string CourseName { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string Status { get; set; } = "Pending"; // Pending / Submitted / Overdue
    }
}
