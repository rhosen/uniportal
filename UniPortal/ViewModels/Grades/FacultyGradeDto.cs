namespace UniPortal.ViewModels.Grades
{
    public class FacultyGradeDto
    {
        public Guid StudentId { get; set; }
        public Guid CourseOfferingId { get; set; }

        public string StudentName { get; set; }
        public string FacultyName { get; set; }

        public string CourseTitle { get; set; }
        public string CourseCode { get; set; }
        public string Grade { get; set; }
        public decimal? Marks { get; set; }
        public decimal? GPA { get; set; }
    }
}
