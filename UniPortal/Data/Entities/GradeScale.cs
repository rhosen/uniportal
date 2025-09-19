namespace UniPortal.Data.Entities
{
    public class GradeScale: IEntity
    {
        public string Grade { get; set; }         // e.g., A+, A, B+
        public decimal MinMarks { get; set; }     // Minimum marks for this grade
        public decimal MaxMarks { get; set; }     // Maximum marks for this grade
        public decimal GPA { get; set; }          // Grade points for GPA
    }
}
