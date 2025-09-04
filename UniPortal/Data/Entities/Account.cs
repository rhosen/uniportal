namespace UniPortal.Data.Entities
{
    public class Account : IEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string IdentityUserId { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string IdentityNumber { get; set; } = string.Empty; 
        public string BatchNumber { get; set; } 
    }
}
