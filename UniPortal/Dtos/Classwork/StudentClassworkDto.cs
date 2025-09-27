namespace UniPortal.Dtos.Classwork
{
    public class StudentClassworkDto
    {
        public Guid Id { get; set; }                 // Coursework ID
        public string Title { get; set; }
        public string Description { get; set; }
        public bool RequiresSubmission { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? UploadedAt { get; set; }
        public bool HasSubmitted { get; set; }      // If the student has submitted
        public string FilePath { get; set; }
    }
}
