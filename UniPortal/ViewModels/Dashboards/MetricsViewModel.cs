namespace UniPortal.ViewModels.Dashboards
{
    public class MetricsViewModel
    {
        public int Courses { get; set; }
        public int PendingAssignments { get; set; }
        public int TodayClasses { get; set; }

        public int AttendancePercent { get; set; }        // e.g., 85 (%)
        public string OverallGPA { get; set; } = "N/A";  // e.g., "3.75" or "N/A"
        public int UnreadNotifications { get; set; }     // number of unread notifications
        public int NotesCount { get; set; }             // number of class notes available
        public int Classrooms { get; set; }             // optional: number of classrooms, can be static if needed
    }

}
