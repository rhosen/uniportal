namespace UniPortal.Data.Entities
{
    public class CourseOffering : IEntity
    {
        public Guid CurriculumId { get; set; }
        public Guid CourseId { get; set; }
        public Guid SemesterId { get; set; }
        public int SemesterNumber { get; set; }
        public Guid ProgramId { get; set; }
        public Guid BatchId { get; set; }
        public Guid SectionId { get; set; }
        public Guid FacultyId { get; set; }
        public int CreditHours { get; set; }
        public int SequenceOrder { get; set; }
        public int MaxEnrollment { get; set; }
        public int CurrentEnrollment { get; set; } = 0;
        public bool Mon { get; set; } = false;
        public bool Tue { get; set; } = false;
        public bool Wed { get; set; } = false;
        public bool Thu { get; set; } = false;
        public bool Fri { get; set; } = false;
        public bool Sat { get; set; } = false;
        public bool Sun { get; set; } = false;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public Guid RoomId { get; set; }
    }
}
