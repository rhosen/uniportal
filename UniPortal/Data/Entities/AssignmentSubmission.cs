namespace UniPortal.Data.Entities
{
    public class AssignmentSubmission : IEntity
    {
        public Guid AssignmentId { get; set; }
        public Guid StudentId { get; set; }
        public DateTime SubmittedDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending";
        public decimal? MarksAwarded { get; set; }


        // Navigation
        public Assignment Assignment { get; set; }
        public Student Student { get; set; }
    }
}
