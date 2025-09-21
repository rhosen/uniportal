namespace UniPortal.Dtos
{
    public class StudentDto
    {
        public Guid Id { get; set; }
        public string StudentNumber { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public string ProgramName { get; internal set; }
        public int CurrentSemester { get; internal set; }
        public string CurrentSemesterName { get; internal set; }
    }
}
