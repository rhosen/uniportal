namespace UniPortal.Dtos
{
    public class ClassroomStatusDto
    {
        public Guid Id { get; set; }
        public string RoomName { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
        public bool IsOccupied { get; set; }
        public List<ScheduleDto> CurrentSchedules { get; set; } = new();
    }
}
