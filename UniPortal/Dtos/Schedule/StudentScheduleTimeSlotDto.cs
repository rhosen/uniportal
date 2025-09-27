namespace UniPortal.Dtos.Schedule
{
    public class StudentScheduleTimeSlotDto
    {
        public int Hour { get; set; }
        public TimeSpan Start => new(Hour, 0, 0);
        public string Label => DateTime.Today.Add(Start).ToString("hh:mm tt");
    }
}
