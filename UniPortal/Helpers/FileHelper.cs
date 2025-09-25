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
                UploadType.Notice => "uploads/notices",
                UploadType.ClassMaterial => "uploads/classmaterials",
                UploadType.Assignment => "uploads/assignments",
                UploadType.Submission => "uploads/submissions",
                _ => "uploads/others"
            };

            // Add courseOfferingId for course-specific files
            if (!string.IsNullOrEmpty(path) &&
                (type == UploadType.ClassMaterial || type == UploadType.Assignment || type == UploadType.Submission))
            {
                baseFolder = Path.Combine(baseFolder, path);
            }

            // Add date-based folders
            var dateFolder = Path.Combine(
                DateTime.Now.Year.ToString(),
                DateTime.Now.Month.ToString("D2"),
                DateTime.Now.Day.ToString("D2")
            );

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
