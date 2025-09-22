namespace UniPortal.Dtos
{
    public class EnrollmentCourseDto
    {
        public Guid Id { get; set; }
        public string FacultyName { get; set; }
        public int CreditHours { get; set; }
        public string CourseTitle { get; set; }
        public bool IsEnrolled { get; set; }
    }
}
