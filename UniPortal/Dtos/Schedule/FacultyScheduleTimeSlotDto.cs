namespace UniPortal.Dtos.Schedule
{
    public class FacultyScheduleTimeSlotDto
    {
        public int Hour { get; set; }
        public string Label => $"{Hour}:00";
        public TimeSpan Start => TimeSpan.FromHours(Hour);
    }
}
