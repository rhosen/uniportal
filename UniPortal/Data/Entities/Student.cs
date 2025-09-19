using System.ComponentModel.DataAnnotations.Schema;

namespace UniPortal.Data.Entities
{
    public class Student : IEntity
    {
        public Guid AccountId { get; set; }
        public string StudentId { get; set; } 
        public string BatchNumber { get; set; } 
        public Guid ProgramId { get; set; }
        public string Section { get; set; }
        public Guid? CurrentSemesterId { get; set; }

        // Navigation
        public Account Account { get; set; }
        public Program Program { get; set; }

        [ForeignKey(nameof(CurrentSemesterId))] 
        public Semester Semester { get; set; }

    }
}
