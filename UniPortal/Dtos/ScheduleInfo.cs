namespace UniPortal.Dtos
{
    public class ScheduleInfo
    {
        public string SubjectCode { get; set; }
        public string SubjectName { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
