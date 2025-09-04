namespace UniPortal.Data.Entities
{
    public abstract class IEntity
    {
        public Guid Id { get; set; }
        public Guid? CreatedById { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
