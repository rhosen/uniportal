namespace UniPortal.ViewModels.Classes
{
    public class ScheduleInputModel
    {
        public Guid ScheduleId { get; set; } // for edit
        public Guid SelectedCourseId { get; set; }
        public Guid SelectedClassroomId { get; set; }
        public List<int> SelectedDays { get; set; } = new(); // multi-day create
        public int DayOfWeek { get; set; } // single entry edit
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
