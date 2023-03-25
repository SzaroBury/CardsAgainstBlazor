namespace BlazorCardsAgainstMatey.Data
{
    public class Player : User
    {
        public int RoomId { get; set; }
        public List<Card> Hand { get; set; } = new List<Card>();
        public int Score { get; set; } = 0;

        public Player(User user) : base()
        {
            Id = user.Id;
            Name = user.Name;
            JoinedRooms = user.JoinedRooms;
        }
    }
}
