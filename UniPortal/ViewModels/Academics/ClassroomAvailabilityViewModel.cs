using UniPortal.Dtos;

namespace UniPortal.ViewModels.Academics
{
    public class ClassroomAvailabilityViewModel
    {
        public Guid Id { get; set; }
        public string RoomName { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
        public bool IsOccupied { get; set; }
        public List<ScheduleInfo> CurrentSchedules { get; set; } = new();
    }
}
