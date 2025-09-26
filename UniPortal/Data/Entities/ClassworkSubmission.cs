namespace UniPortal.Data.Entities
{
    public class ClassworkSubmission: IEntity
    {
        public Guid ClassworkId { get; set; }
        public Guid StudentId { get; set; }
        public string FilePath { get; set; }
        public string Remarks { get; set; }
    }
}
