namespace BlazorCardsAgainstMatey.Data
{
    public class RoomService
    {
        private List<Room> _rooms = new List<Room>();

        public Task<List<Room>> GetAllRoomsAsync()
        {
            return Task.FromResult(_rooms);
        }

        public Task<Room?> GetRoomAsync(int id)
        {
            if (!_rooms.Exists(r => r.Id == id)) return null;
            return Task.FromResult(_rooms.Find(r => r.Id == id));
        }
        public Task<bool> CheckRoomExistence(int room_id)
        {
            return Task.FromResult(_rooms.Exists(r => r.Id == room_id));
        }

        public async Task<bool> PlayerJoinAsync(int roomId, User user)
        {
            var index = _rooms.FindIndex(r => r.Id == roomId);
            if (index == -1) return await Task.FromResult(false);

            _rooms[index].Players.Add(user);
            return await Task.FromResult(true);
        }

        public async Task<Room> NewGameAsync(int roomId, int cardsInHand, int scoreToWin)
        {
            var index = _rooms.FindIndex(r => r.Id == roomId);
            Game newGame = new Game(roomId, _rooms[index].Sentences, _rooms[index].Cards, cardsInHand, scoreToWin);

            //check for number of cards
            if (_rooms[index].Players.Count * cardsInHand > newGame.GameCards.Count) return null;

            foreach (var user in _rooms[index].Players)
            {
                Player newPlayer = new Player(user)
                {
                    RoomId = _rooms[index].Id,
                };
                for (int i = 0; i < cardsInHand; i++)
                {
                    if(newGame.GameCards.Count > 0)
                    {
                        newPlayer.Hand.Add(newGame.GameCards[0]);
                        newGame.GameCards.RemoveAt(0);
                    }
                }
                newGame.Players.Add(newPlayer);
            }

            //random chooser
            //newGame.ChooserId = _rooms[index].Players[Random.Shared.Next(0, _rooms[index].Players.Count - 1)].Id;

            //first player as chooser
            newGame.ChooserId = _rooms[index].Players[0].Id;

            _rooms[index].Game = newGame;
            _rooms[index].State = RoomState.Ingame;

            return await Task.FromResult(_rooms[index]);
        }

        public Task<string> AddRoomAsync(string roomName, Guid userId, string userName, int maxPlayers)
        {
            if (_rooms.Exists(r => r.Name == roomName)) return Task.FromResult("Error:Room name already taken.");

            var room = new Room()
            {
                Id = _rooms.Count == 0 ? 0 : _rooms.Max(u => u.Id) + 1,
                Name = roomName,
                OwnerId = userId,
                OwnerName = userName,
                MaxPlayers = maxPlayers,
                State = RoomState.New
            };
            _rooms.Add(room);

            return Task.FromResult($"roomId:{room.Id}");
        }

        public Task<Room> SendAnswerAsync(int roomId, Guid playerId, List<Card> cards)
        {
            var roomIndex = _rooms.FindIndex(r => r.Id == roomId);
            if (roomIndex == -1) return null;

            var playerIndex = _rooms[roomIndex].Players.FindIndex(r => r.Id == playerId);
            if (playerIndex == -1) return null;

            _rooms[roomIndex].Game.ChosenCards.Add(new ChosenCards(playerId, cards));

            foreach(var card in cards)
            {
                _rooms[roomIndex].Game.Players[playerIndex].Hand.Remove(card);
            }

            //if all players picked
            if(_rooms[roomIndex].Game.ChosenCards.Count == _rooms[roomIndex].Game.Players.Count - 1)
            {
                _rooms[roomIndex].Game.State = GameState.ShowCards;
            }

            return Task.FromResult(_rooms[roomIndex]);
        }

        public Task<Room> SendAnswerChooserAsync(int roomId, Guid playerId, List<Card> cards)
        {
            var roomIndex = _rooms.FindIndex(r => r.Id == roomId);
            if (roomIndex == -1) return null;

            var playerIndex = _rooms[roomIndex].Players.FindIndex(r => r.Id == playerId);
            if (playerIndex == -1) return null;

            var cardsIndex = _rooms[roomIndex].Game.ChosenCards.FindIndex(c => c.Cards[0].Id == cards[0].Id);
            if (cardsIndex == -1) return null;
            var winnerCardSet = _rooms[roomIndex].Game.ChosenCards[cardsIndex];
            winnerCardSet.Winner = true;
            _rooms[roomIndex].Game.ChosenCards[cardsIndex] = winnerCardSet;

            var winnerPlayerIndex = _rooms[roomIndex].Game.Players.FindIndex(p => p.Id == _rooms[roomIndex].Game.ChosenCards[cardsIndex].PlayerId);
            _rooms[roomIndex].Game.Players[winnerPlayerIndex].Score++;
            
            if(_rooms[roomIndex].Game.Players.Any(p => p.Score == _rooms[roomIndex].Game.ScoreToWin))
            {
                _rooms[roomIndex].Game.State = GameState.Finished;
                _rooms[roomIndex].State = RoomState.Finished;
            }
            else
            {
                _rooms[roomIndex].Game.State = GameState.ShowWinnerCards;
            }

            return Task.FromResult(_rooms[roomIndex]);
        }

        public Task<Room> NewRoundAsync(int roomId)
        {
            var roomIndex = _rooms.FindIndex(r => r.Id == roomId);
            if (roomIndex == -1) return null;

            //new cards for players
            for (int i = 0; i < _rooms[roomIndex].Game.CurrentSentence.BlankFields; i++)
            {
                foreach(var pl in _rooms[roomIndex].Game.Players)
                {
                    if (_rooms[roomIndex].Game.GameCards.Count == 0)
                    {
                        //take all cards and copy to gamecards
                        _rooms[roomIndex].Game.GameCards = _rooms[roomIndex].Cards.OrderBy(key => Random.Shared.Next()).ToList();

                        //remove cards that players already have
                        foreach (var p in _rooms[roomIndex].Game.Players)
                        {
                            foreach (var c in p.Hand)
                            {
                                _rooms[roomIndex].Game.GameCards.Remove(c);
                            }
                        }
                    }

                    if(pl.Id != _rooms[roomIndex].Game.ChooserId)
                    {
                        pl.Hand.Add(_rooms[roomIndex].Game.GameCards[0]);
                        _rooms[roomIndex].Game.GameCards.RemoveAt(0);
                    }
                }
            }

            //new chooser
            var chooserIndex = _rooms[roomIndex].Game.Players.FindIndex(p => p.Id == _rooms[roomIndex].Game.ChooserId);
            if(chooserIndex == _rooms[roomIndex].Game.Players.Count - 1)
            {
                _rooms[roomIndex].Game.ChooserId = _rooms[roomIndex].Players[0].Id;
            }
            else
            {
                _rooms[roomIndex].Game.ChooserId = _rooms[roomIndex].Players[chooserIndex + 1].Id;
            }

            //new sentence
            if(_rooms[roomIndex].Game.GameSentences.Count == 0)
            {
                _rooms[roomIndex].Game.GameSentences = _rooms[roomIndex].Sentences.OrderBy(key => Random.Shared.Next()).ToList();
                var sentenceIndex = _rooms[roomIndex].Game.GameSentences.FindIndex(s => s.Id == _rooms[roomIndex].Game.CurrentSentence.Id);
                _rooms[roomIndex].Game.GameSentences.RemoveAt(sentenceIndex);
                
            }

            _rooms[roomIndex].Game.CurrentSentence = _rooms[roomIndex].Game.GameSentences[0];
            _rooms[roomIndex].Game.GameSentences.RemoveAt(0);
            

            //other simple fields
            _rooms[roomIndex].Game.Round++;
            _rooms[roomIndex].Game.State = GameState.PickCards;
            _rooms[roomIndex].Game.ChosenCards.Clear();

            return Task.FromResult(_rooms[roomIndex]);
        }
    
        public Task<bool> AddCardAsync(int roomId, string cardValue)
        {
            var roomIndex = _rooms.FindIndex(r => r.Id == roomId);
            if (roomIndex == -1) return Task.FromResult(false);

            int newCardId = _rooms[roomIndex].Cards.OrderByDescending(c => c.Id).FirstOrDefault().Id + 1;
            _rooms[roomIndex].Cards.Add(new Card { Id = newCardId, Value = cardValue });

            return Task.FromResult(true);
        }

        public Task<bool> RemoveCardAsync(int roomId, int cardId)
        {
            var roomIndex = _rooms.FindIndex(r => r.Id == roomId);
            if (roomIndex == -1) return Task.FromResult(false);

            var cardIndex = _rooms[roomIndex].Cards.FindIndex(c => c.Id == cardId);
            if (cardIndex == -1) return Task.FromResult(false);
            _rooms[roomIndex].Cards.RemoveAt(cardIndex);

            return Task.FromResult(true);
        }

        public async Task<bool> LoadCardsAsync(int roomId, Stream stream)
        {
            var roomIndex = _rooms.FindIndex(r => r.Id == roomId);
            if (roomIndex == -1)
            {
                return false;
            }

            StreamReader streamReader = new StreamReader(stream);
            string temp = await streamReader.ReadToEndAsync();


            using (StringReader reader = new StringReader(temp))
            {
                string line = reader.ReadLine();
                while (line != null)
                {
                    if(!_rooms[roomIndex].Cards.Exists(c => c.Value == line))
                    {
                        int newId = _rooms[roomIndex].Cards.OrderByDescending(c => c.Id).FirstOrDefault().Id + 1;
                        _rooms[roomIndex].Cards.Add(new Card { Id = newId, Value = line.Trim() });
                    }
                    line = reader.ReadLine();
                }
            }

            return true;
        }

        public Task<bool> ClearCardsAsync(int roomId)
        {
            var roomIndex = _rooms.FindIndex(r => r.Id == roomId);
            if (roomIndex == -1)
            {
                return Task.FromResult(false);
            }

            _rooms[roomIndex].Cards.Clear();
            return Task.FromResult(true);
        }

        public Task<bool> AddSentenceAsync(int roomId, string newSentenceValue, int newSentenceBlanks)
        {
            var roomIndex = _rooms.FindIndex(r => r.Id == roomId);
            if (roomIndex == -1) return Task.FromResult(false);

            int newSentenceId = _rooms[roomIndex].Sentences.OrderByDescending(c => c.Id).FirstOrDefault().Id + 1;
            _rooms[roomIndex].Sentences.Add(new Sentence { Id = newSentenceId, Value = newSentenceValue, BlankFields = newSentenceBlanks });

            return Task.FromResult(true);
        }

        public Task<bool> RemoveSentenceAsync(int roomId, int sentenceId)
        {
            var roomIndex = _rooms.FindIndex(r => r.Id == roomId);
            if (roomIndex == -1) return Task.FromResult(false);

            var sentenceIndex = _rooms[roomIndex].Sentences.FindIndex(c => c.Id == sentenceId);
            if (sentenceIndex == -1) return Task.FromResult(false);
            _rooms[roomIndex].Sentences.RemoveAt(sentenceIndex);

            return Task.FromResult(true);
        }

        public async Task<bool> LoadSentencesAsync(int roomId, Stream stream)
        {
            var roomIndex = _rooms.FindIndex(r => r.Id == roomId);
            if (roomIndex == -1)
            {
                return false;
            }

            StreamReader streamReader = new StreamReader(stream);
            string temp = await streamReader.ReadToEndAsync();


            using (StringReader reader = new StringReader(temp))
            {
                string line = reader.ReadLine();
                while (line != null)
                {
                    var lineSplit = line.Split(";");
                    if(lineSplit[1] == null)
                    {
                        return false;
                    }
                    if (!_rooms[roomIndex].Sentences.Exists(c => c.Value == lineSplit[0]))
                    {
                        int newId = _rooms[roomIndex].Sentences.OrderByDescending(c => c.Id).FirstOrDefault().Id + 1;
                        _rooms[roomIndex].Sentences.Add(new Sentence { Id = newId, Value = lineSplit[0], BlankFields = Convert.ToInt32(lineSplit[1]) });
                    }
                    line = reader.ReadLine();
                }
            }

            return true;
        }

        public Task<bool> ClearSentencesAsync(int roomId)
        {
            var roomIndex = _rooms.FindIndex(r => r.Id == roomId);
            if (roomIndex == -1)
            {
                return Task.FromResult(false);
            }

            _rooms[roomIndex].Sentences.Clear();
            return Task.FromResult(true);
        }
    }
}