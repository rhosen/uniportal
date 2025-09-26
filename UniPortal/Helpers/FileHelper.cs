using UniPortal.Constants;

namespace UniPortal.Helpers
{
    public static class FileHelper
    {
        public static async Task<string> SaveFileAsync(
            IFormFile file,
            UploadType type,
            string? path = null) // For courseOfferingId
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            // Base folder
            string baseFolder = type switch
            {
                UploadType.Notification => "uploads/notifications",
                UploadType.Classwork => "uploads/classworks",
                UploadType.Submission => "uploads/submissions",
                _ => "uploads/others"
            };

            // Add courseOfferingId for course-specific files
            if (!string.IsNullOrEmpty(path) &&
                (type == UploadType.Classwork || type == UploadType.Assignment || type == UploadType.Submission))
            {
                baseFolder = Path.Combine(baseFolder, path);
            }

            // Add single date folder (yyyy-MM-dd)
            var dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
            baseFolder = Path.Combine(baseFolder, dateFolder);

            // Ensure folder exists
            Directory.CreateDirectory(Path.Combine("wwwroot", baseFolder));

            // Save file
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var fileFullPath = Path.Combine("wwwroot", baseFolder, fileName);

            using var stream = new FileStream(fileFullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Return relative path for db
            return $"/{baseFolder.Replace("\\", "/")}/{fileName}";
        }
    }
}
