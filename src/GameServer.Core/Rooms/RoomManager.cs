using System.Collections.Concurrent;

namespace GameServer.Core.Rooms
{
    public class RoomManager
    {
        private readonly ConcurrentDictionary<string, Room> _rooms = new();

        public Room CreateRoom(string name)
        {
            var room = new Room(name);
            _rooms[room.Id] = room;
            return room;
        }

        public bool TryGetRoom(string id, out Room? room)
        {
            return _rooms.TryGetValue(id, out room);
        }

        public bool RemoveRoom(string id)
        {
            return _rooms.TryRemove(id, out _);
        }

        public IEnumerable<Room> GetAllRooms()
        {
            return _rooms.Values;
        }
    }
}
