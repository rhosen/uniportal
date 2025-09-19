namespace UniPortal.Dtos
{
    public class StudentDto
    {
        public Guid Id { get; set; }
        public string StudentId { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public string ProgramName { get; internal set; }
        public Guid? CurrentSemesterId { get; internal set; }
        public string CurrentSemesterName { get; internal set; }
    }
}
