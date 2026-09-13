using GameServer.Core.Networking;

namespace GameServer.Core.Rooms
{
    public class Room
    {
        public string Id { get; }
        public int MaxPlayers { get; }
        public Dictionary<string, ClientSession> Players { get; } = new();
        public object? State { get; set; }

        public Room(string id, int maxPlayers = 10)
        {
            Id = id;
            MaxPlayers = maxPlayers;
        }

        public bool AddPlayer(ClientSession session)
        {
            if (Players.Count >= MaxPlayers) return false;
            Players[session.Id] = session;
            return true;
        }

        public void RemovePlayer(string sessionId)
        {
            Players.Remove(sessionId);
        }

        public bool IsEmpty => Players.Count == 0;
    }
}
