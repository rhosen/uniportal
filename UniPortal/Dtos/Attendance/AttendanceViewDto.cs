namespace UniPortal.Dtos.Attendance
{
    public class AttendanceViewDto
    {
        public Guid CourseOfferingId { get; set; }
        public DateTime Date { get; set; }
        public bool IsCanceled { get; set; } = false;
        public string? CancellationReason { get; set; }
        public List<AttendanceRowDto> Students { get; set; } = new();
    }
}
