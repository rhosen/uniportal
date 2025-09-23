namespace UniPortal.Dtos
{
    public class CourseOfferingDto
    {
        public Guid CurriculumId { get; set; }
        public Guid CourseId { get; set; }
        public string CourseTitle { get; set; }
        public int CreditHours { get; set; }
        public int Sequence { get; set; }

        // Editable fields for offering
        public Guid FacultyId { get; set; }
        public string FacultyName { get; set; }
        public Guid RoomId { get; set; }
        public string RoomName { get; set; }
        public int MaxEnrollment { get; set; }
        public bool Mon { get; set; } = false;
        public bool Tue { get; set; } = false;
        public bool Wed { get; set; } = false;
        public bool Thu { get; set; } = false;
        public bool Fri { get; set; } = false;
        public bool Sat { get; set; } = false;
        public bool Sun { get; set; } = false;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public bool IsOffered { get; set; } = false; // True if already offered
        public Guid? OfferingId { get; set; } // Existing offering ID
    }
}
