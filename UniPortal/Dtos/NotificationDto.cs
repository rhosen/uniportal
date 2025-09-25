namespace UniPortal.Dtos
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        // Recipient info
        public Guid RecipientTypeId { get; set; }
        public string? RecipientTypeName { get; set; }   // e.g. "Student", "Faculty", "All"
        public string? RecipientNumber { get; set; }     // human friendly number (student/faculty)
        public Guid? AccountId { get; set; }             // resolved AccountId (nullable for "All")

        // file, timestamps, soft delete
        public string? FilePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsRead { get; set; }
    }
}
