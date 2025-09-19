namespace UniPortal.Dtos
{
    public class CourseDto
    {
        public Guid Id { get; set; }
        public string SubjectCode { get; set; }
        public string SubjectName { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public string TeacherName { get; set; }
        public int Credits { get; set; }
    }
}
