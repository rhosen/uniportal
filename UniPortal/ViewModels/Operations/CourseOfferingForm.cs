using Microsoft.AspNetCore.Mvc;
using UniPortal.Dtos;

namespace UniPortal.ViewModels.Operations
{
    public class CourseOfferingForm
    {
        public Guid ProgramId { get; set; }
        public int SemesterNumber { get; set; } // 1-8 configurable
        public Guid BatchId { get; set; }
        public Guid SectionId { get; set; }

        // Courses from curriculum + existing offerings
        public List<CourseOfferingDto> Courses { get; set; } = new();
    }
}
