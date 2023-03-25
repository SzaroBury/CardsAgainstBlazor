namespace BlazorCardsAgainstMatey.Data
{
    public class User
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public List<JoinedRoom> JoinedRooms { get; set; } = new List<JoinedRoom>();

        public bool JoinedRoomsChanged = false;

    }

    public struct JoinedRoom
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
