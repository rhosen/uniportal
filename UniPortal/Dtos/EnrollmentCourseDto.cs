namespace UniPortal.Dtos
{
    public class EnrollmentCourseDto
    {
        public Guid Id { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
        public int CreditHours { get; set; }
        public int MaxEnrollment { get; set; }
        public int CurrentEnrollment { get; set; }
        public string Schedule { get; set; } = string.Empty;
        public bool IsEnrolled { get; set; } = false;
        public bool IsGraded { get; set; } = false;
    }
}
