namespace UniPortal.Dtos.Schedule
{
    public class FacultyScheduleCourseDto
    {
        public Guid CourseOfferingId { get; set; }
        public string Code { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Batch { get; set; } = null!;
        public string Section { get; set; } = null!;
        public string Room { get; set; } = null!;
        public int Credits { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime Date { get; set; } // exact calendar date
        public bool IsCancelled { get; set; }
        public string? CancellationReason { get; set; }

        public string Time => $"{StartTime:hh\\:mm}–{EndTime:hh\\:mm}";
        public string Day => Date.ToString("ddd");
    }
}
