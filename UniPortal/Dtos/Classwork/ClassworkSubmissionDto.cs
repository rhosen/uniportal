namespace UniPortal.Dtos.Classwork
{
    public class ClassworkSubmissionDto
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }   // For display in sidebar
        public string FilePath { get; set; }      // Optional attachment
        public string Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
