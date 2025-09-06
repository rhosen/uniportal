using System.ComponentModel.DataAnnotations.Schema;

namespace UniPortal.Data.Entities
{
    public class Student : IEntity
    {
        public Guid AccountId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public string Section { get; set; } = string.Empty;  // New property

        // Navigation
        [ForeignKey(nameof(AccountId))]
        public Account StudentAccount { get; set; }
        public Department Department { get; set; }
    }
}
