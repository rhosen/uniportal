namespace UniPortal.Data.Entities
{
    public class Room : IEntity
    {
        public string RoomName { get; set; } = string.Empty;
        public int Capacity { get; set; } = 30;
        public string? Location { get; set; }       // nullable
    }
}
