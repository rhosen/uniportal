namespace UniPortal.Dtos
{
    public class ScheduleDto
    {
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
    }
}
