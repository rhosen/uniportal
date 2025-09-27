namespace UniPortal.Dtos.Attendance
{
    public class AttendanceReportDto
    {
        public DateTime Date { get; set; }
        public string Status { get; set; } = "Absent";
        public string Remarks { get; set; } = "";
        public bool IsCanceled { get; set; }
        public string? CancellationReason { get; set; }
    }
}
