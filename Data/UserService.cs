namespace BlazorCardsAgainstMatey.Data
{
    public class UserService
    {
        public UserService(RoomService roomService)
        {
            _roomService = roomService;
        }

        private List<User> _users = new List<User>();
        private RoomService _roomService;

        public Task<List<User>> GetAllUsersAsync()
        {
            return Task.FromResult(_users);
        }

        public Task<User?> GetUserAsync(Guid user_id)
        {
            return Task.FromResult(_users.Find(u => u.Id == user_id));
        }

        public Task<bool> CheckNameUniqueness(string username)
        {
            if(_users.Exists(u => u.Name == username))  return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public Task<bool> CheckUserExistence(Guid user_id)
        {
            return Task.FromResult(_users.Exists(u => u.Id == user_id));
        }

        public Task<string> GetUserName(Guid userId)
        {
            var result = _users.Find(u => u.Id == userId).Name;
            return Task.FromResult(result);
        }

        public Task<List<JoinedRoom>> GetUserJoinedRoom(Guid? userId)
        {
            if(!userId.HasValue) return Task.FromResult(new List<JoinedRoom>());

            var index = _users.FindIndex(u => u.Id == userId);

            var result = _users[index].JoinedRooms;
            return Task.FromResult(result);
        }

        public Task<User>? JoinRoom(Guid? userId, int? roomId, string roomName)
        {
            //check for nulls
            if (!userId.HasValue)
            {
                throw new Exception("JoinRoom(): userId cannot be null.");
            }
            if (!roomId.HasValue)
            {
                throw new Exception("JoinRoom(): roomId cannot be null.");
            }

            //check for existence
            var index = _users.FindIndex(u => u.Id == userId);
            if (index == -1)
            {
                throw new Exception("JoinRoom(): userId not found in system.");
            }

            _users[index].JoinedRooms.Add(new JoinedRoom { Id = roomId.Value, Name = roomName });
            _roomService.PlayerJoinAsync(roomId.Value, _users[index]);
            _users[index].JoinedRoomsChanged = true;

            return Task.FromResult(_users[index]);
        }

        public Task<Guid> Login(string username)
        {
            Console.WriteLine($"_users.Count: {_users.Count}");
            User new_u = new User
            {
                Id = Guid.NewGuid(), //_users.Count == 0 ? 0 :_users.Max(u => u.Id) + 1,
                Name = username,
            };
            _users.Add(new_u);

            Console.WriteLine($"New user logged in: {username}");
            Console.WriteLine($"_users.Count: {_users.Count}");

            return Task.FromResult(new_u.Id);
        }
    }
}