namespace UniPortal.ViewModels.Grades
{
    public class TeacherGradeViewModel
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public Guid CourseId { get; set; }
        public Guid SubjectId { get; set; }

        public string SubjectName { get; set; }
        public string SubjectCode { get; set; }
        public string TeacherName { get; set; }
        public string Grade { get; set; }
        public decimal? Marks { get; set; }
        public Guid CourseOfferingId { get; set; }
        public decimal? GPA { get; set; }
    }
}
