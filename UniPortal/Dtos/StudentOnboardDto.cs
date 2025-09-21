namespace UniPortal.Dtos
{
    public class StudentOnboardDto
    {
        public Guid AccountId { get; set; }
        public string Email { get; set; } 
        public DateTime CreatedAt { get; set; }
        public string StudentNumber { get; set; }
        public Guid BatchId { get; set; }
        public Guid SectionId { get; set; }
        public Guid ProgramId { get; set; }
        public bool UseSystemId { get; set; } = true;
    }
}
