namespace UniPortal.Data.Entities
{
    public class Institution : IEntity
    {
        public string Name { get; set; } = null!;
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string LogoUrl { get; set; }
    }
}
