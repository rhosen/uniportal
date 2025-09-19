namespace UniPortal.ViewModels.Grades
{
    public class GradeViewModel
    {
        public string SemesterName { get; set; }   
        public string SubjectName { get; set; }
        public string SubjectCode { get; set; }
        public string TeacherName { get; set; }
        public string Grade { get; set; }            
        public decimal Marks { get; set; }          
        public decimal GPA { get; set; }          
        public bool IsFail { get; set; }
    }
}
