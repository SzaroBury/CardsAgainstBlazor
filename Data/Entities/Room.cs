﻿namespace BlazorCardsAgainstMatey.Data
{
    public class Room
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public Guid OwnerId { get; set; }

        public string OwnerName { get; set; } = "";

        public int MaxPlayers { get; set; } = 10;

        public List<User> Players { get; set; } = new List<User>();

        public string PlayersDisplay => string.Format(@"{0}/{1}", Players.Count, MaxPlayers);

        public List<Sentence> Sentences { get; set; } = new List<Sentence>
        {
            new Sentence(0, "Co ____ to ____ .", 2),
            new Sentence(1, "____ czy coś.", 1),
            new Sentence(2, "Tak jak wtedy wszyscy ____ .", 1)
        };

        public List<Card> Cards { get; set; } = new List<Card>
        {
            new Card(0, "zero"),
            new Card(1, "jeden"),
            new Card(2, "dwa"),
            new Card(3, "trzy"),
            new Card(4, "cztery"),
            new Card(5, "pięć"),
            new Card(6, "sześć"),
            new Card(7, "siedem"),
            new Card(8, "osiem"),
            new Card(9, "dziewięć")
        };

        public RoomState State { get; set; }

        public Game? Game { get; set; }
    }

    public enum RoomState
    {
        New,
        Ingame,
        Finished
    }
}