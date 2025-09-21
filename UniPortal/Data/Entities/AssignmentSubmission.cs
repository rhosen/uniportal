using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace UniPortal.Data.Entities
{
    public class AssignmentSubmission : IEntity
    {
        public Guid AssignmentId { get; set; }
        public Guid StudentId { get; set; }
        public DateTime SubmittedDate { get; set; } = DateTime.Now;
        public string FilePath { get; set; } = null!;
        public string Status { get; set; } = "Pending";
        public decimal? MarksAwarded { get; set; }

        // Navigation properties
        public virtual Assignment Assignment { get; set; } = null!;
        public virtual Student Student { get; set; } = null!;
    }
}
