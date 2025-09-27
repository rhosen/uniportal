namespace UniPortal.Dtos.Schedule
{
    public class StudentScheduleCourseDto
    {
        public string Code { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Faculty { get; set; } = null!;
        public string Days { get; set; } = null!;
        public string Time { get; set; } = null!;
        public string Room { get; set; } = null!;
        public int Credits { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        // New properties for cancellation
        public bool IsCancelled { get; set; } = false;
        public string? CancellationReason { get; set; }
        public DateTime? CancellationDate { get; set; }
    }
}
