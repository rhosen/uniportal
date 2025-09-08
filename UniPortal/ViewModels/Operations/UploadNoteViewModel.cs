namespace UniPortal.ViewModels.Operations
{
    public class UploadNoteViewModel
    {
        public Guid CourseId { get; set; }
        public IFormFile File { get; set; } = null!;
    }
}
