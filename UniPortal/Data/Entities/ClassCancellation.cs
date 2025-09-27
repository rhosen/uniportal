namespace UniPortal.Data.Entities
{
    public class ClassCancellation : IEntity
    {
        public Guid CourseOfferingId { get; set; }
        public DateTime CancellationDate { get; set; }
        public string? Reason { get; set; }

    }
}
