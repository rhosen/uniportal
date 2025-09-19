using System.ComponentModel.DataAnnotations;

namespace UniPortal.Dtos
{
    public class StudentOnboardDto
    {
        public Guid AccountId { get; set; }

        public string Email { get; set; } 

        public DateTime CreatedAt { get; set; }

        public string StudentId { get; set; }

        public string? BatchNumber { get; set; }

        public string? Section { get; set; }

        public Guid ProgramId { get; set; }

        public bool UseSystemId { get; set; } = true;
    }

}
