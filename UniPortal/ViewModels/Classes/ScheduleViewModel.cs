using UniPortal.ViewModels.Operations;

namespace UniPortal.ViewModels.Classes
{
    public class ScheduleViewModel
    {
        public Guid ScheduleId { get; set; }
        public string CourseName { get; set; }
        public string ClassroomName { get; set; }
        public List<ScheduleEntryViewModel> Entries { get; set; } = new();
    }
}
