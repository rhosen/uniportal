namespace UniPortal.ViewModel
{
    public class UploadNoteViewModel
    {
        public Guid CourseId { get; set; }
        public IFormFile File { get; set; } = null!;
    }
}
