namespace UniPortal.Dtos.Classwork
{
    public class ClassworkCreateUpdateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public IFormFile File { get; set; }

        public string FilePath { get; set; }

        public bool RequiresSubmission { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
