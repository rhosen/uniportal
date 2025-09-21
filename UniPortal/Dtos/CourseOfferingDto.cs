using System;

namespace UniPortal.Dtos
{
    public class CourseOfferingDto
    {
        public Guid Id { get; set; }                // Existing offering Id (for edit/delete)
        public Guid CurriculumId { get; set; }      // Optional reference to original curriculum
        public Guid CourseId { get; set; }          // Snapshot from curriculum
        public string CourseTitle { get; set; }
        public string CourseType { get; set; }
        public Guid CourseTypeId { get; set; }
        public Guid SemesterId { get; set; }        // Snapshot from curriculum
        public int SemesterNumber { get; set; }
        public Guid ProgramId { get; set; }         // Snapshot from curriculum
        public Guid BatchId { get; set; }
        public Guid SectionId { get; set; }
        public Guid FacultyId { get; set; }         // Optional teacher/faculty
        public string FacultyName { get; set; }     // Display name

        public int CreditHours { get; set; }        // Snapshot from curriculum
        public int SequenceOrder { get; set; }      // Snapshot from curriculum
        public int MaxEnrollment { get; set; }
        public int CurrentEnrollment { get; set; } = 0;

        // Schedule snapshot (toggle buttons)
        public bool Mon { get; set; }
        public bool Tue { get; set; }
        public bool Wed { get; set; }
        public bool Thu { get; set; }
        public bool Fri { get; set; }
        public bool Sat { get; set; }
        public bool Sun { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public Guid? RoomId { get; set; }
    }
}
