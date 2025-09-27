namespace UniPortal.Dtos.Classwork
{
    public class StudentClassworkDetailsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool RequiresSubmission { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? UploadedAt { get; set; }

        // Submission info for student
        public string SubmittedFilePath { get; set; }
        public string Remarks { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public bool HasSubmitted { get; set; }
        public string FilePath { get; set; }
    }
}
