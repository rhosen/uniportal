namespace UniPortal.Dtos
{
    public class StudenGradeDto
    {
        public string SemesterName { get; set; }   
        public string CourseTitle { get; set; }
        public string CourseCode { get; set; }
        public string FacultyName { get; set; }
        public string Grade { get; set; }            
        public decimal Marks { get; set; }          
        public decimal GPA { get; set; }          
        public bool IsFail { get; set; }
    }
}
