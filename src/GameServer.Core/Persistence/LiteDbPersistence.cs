using LiteDB;

namespace GameServer.Core.Persistence
{
    public class LiteDbPersistence : IDisposable
    {
        private readonly LiteDatabase _db;
        private readonly ILiteCollection<PlayerData> _players;
        private readonly ILiteCollection<RoomData> _rooms;
        private readonly ILiteCollection<GameStateData> _gameStates;

        public LiteDbPersistence(string dbPath = "gameserver.litedb")
        {
            _db = new LiteDatabase(dbPath);
            _players = _db.GetCollection<PlayerData>("players");
            _rooms = _db.GetCollection<RoomData>("rooms");
            _gameStates = _db.GetCollection<GameStateData>("gamestates");

            _players.EnsureIndex(x => x.Username, unique: true);
            _rooms.EnsureIndex(x => x.Name, unique: true);
            _gameStates.EnsureIndex(x => x.EntityId);
        }

        public void SavePlayer(PlayerData player)
        {
            var existing = _players.FindOne(x => x.Id == player.Id);
            if (existing != null)
            {
                _players.Update(player);
            }
            else
            {
                _players.Insert(player);
            }
        }

        public PlayerData? GetPlayer(string playerId)
        {
            return _players.FindById(playerId);
        }

        public PlayerData? GetPlayerByUsername(string username)
        {
            return _players.FindOne(x => x.Username == username);
        }

        public void DeletePlayer(string playerId)
        {
            _players.Delete(playerId);
        }

        public void SaveRoom(RoomData room)
        {
            var existing = _rooms.FindById(room.Id);
            if (existing != null)
            {
                _rooms.Update(room);
            }
            else
            {
                _rooms.Insert(room);
            }
        }

        public RoomData? GetRoom(string roomId)
        {
            return _rooms.FindById(roomId);
        }

        public void DeleteRoom(string roomId)
        {
            _rooms.Delete(roomId);
        }

        public void SaveGameState(GameStateData gameState)
        {
            var existing = _gameStates.FindOne(x => x.Id == gameState.Id);
            if (existing != null)
            {
                _gameStates.Update(gameState);
            }
            else
            {
                _gameStates.Insert(gameState);
            }
        }

        public GameStateData? GetGameState(Guid gameStateId)
        {
            return _gameStates.FindById(gameStateId);
        }

        public void DeleteGameState(Guid gameStateId)
        {
            _gameStates.Delete(gameStateId);
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
