namespace UniPortal.Dtos
{
    public class StudentDto
    {
        public Guid Id { get; set; }
        public string StudentNumber { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public string ProgramName { get; set; }
        public string Section { get; set; }
        public string Batch { get; set; }
        public int CurrentSemester { get; set; }
        public string CurrentSemesterName { get; set; }
    }
}
