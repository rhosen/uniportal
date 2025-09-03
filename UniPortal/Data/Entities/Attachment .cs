namespace UniPortal.Data.Entities
{
    public class Attachment : IEntity
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? FileType { get; set; }
        public Guid UploadedById { get; set; }
        public string? RelatedEntity { get; set; } // "ClassNote", "Assignment", etc.
        public Guid? RelatedEntityId { get; set; }
    }

}
