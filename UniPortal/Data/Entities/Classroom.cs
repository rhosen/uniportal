namespace UniPortal.Data.Entities
{
    public class Classroom : IEntity
    {
        public string RoomName { get; set; } = string.Empty;
        public int Capacity { get; set; } = 30;
        public string Location { get; set; } = string.Empty;
    }
}
