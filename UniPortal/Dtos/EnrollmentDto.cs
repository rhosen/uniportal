namespace UniPortal.Dtos
{
    public class EnrollmentDto
    {
        public string Id { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string Department { get; set; }
        public string CourseName { get; set; }
        public string TeacherName { get; set; }
        public int Credits { get; set; }
        public bool IsDeleted { get; set; }
    }
}
