using System.ComponentModel.DataAnnotations.Schema;

namespace UniPortal.Data.Entities
{
    public class Enrollment : IEntity
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }

        [NotMapped]
        public new DateTime? UpdatedAt { get; set; }


        // Navigation
        public Student Student { get; set; }
        public Course Course { get; set; }
    }
}
