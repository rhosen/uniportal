namespace UniPortal.Dtos.Attendance
{
    public class AttendanceRowDto
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = null!;
        public string Status { get; set; } = "Absent";
        public string Remarks { get; set; } = "";
    }
}
