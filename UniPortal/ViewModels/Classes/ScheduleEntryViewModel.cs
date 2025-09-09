namespace UniPortal.ViewModels.Classes
{
    public class ScheduleEntryViewModel
    {
        public Guid EntryId { get; set; }
        public int DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string DayName => System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.DayNames[DayOfWeek];

    }
}
