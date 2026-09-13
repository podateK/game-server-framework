using System.Numerics;
using GameServer.Core.Spatial;

namespace GameServer.Core.Rooms
{
    public class GameRoom
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        private readonly SpatialGrid<GameObject> _spatialGrid = new(50f);
        private readonly Dictionary<string, GameObject> _gameObjects = new();
        private readonly object _lock = new();

        public GameRoom(string name)
        {
            Name = name;
        }

        public void AddGameObject(GameObject obj)
        {
            lock (_lock)
            {
                _gameObjects[obj.Id] = obj;
                _spatialGrid.AddOrUpdate(obj);
            }
        }

        public void RemoveGameObject(string id)
        {
            lock (_lock)
            {
                if (_gameObjects.TryGetValue(id, out var obj))
                {
                    _spatialGrid.Remove(obj);
                    _gameObjects.Remove(id);
                }
            }
        }

        public void UpdateGameObjectPosition(string id, Vector2 newPosition)
        {
            lock (_lock)
            {
                if (_gameObjects.TryGetValue(id, out var obj))
                {
                    obj.UpdatePosition(newPosition);
                    _spatialGrid.AddOrUpdate(obj);
                }
            }
        }

        public List<GameObject> GetNearbyObjects(Vector2 position, float radius)
        {
            return _spatialGrid.GetNearby(position, radius);
        }

        public GameObject? GetGameObject(string id)
        {
            lock (_lock)
            {
                return _gameObjects.TryGetValue(id, out var obj) ? obj : null;
            }
        }

        public List<GameObject> GetAllGameObjects()
        {
            lock (_lock)
            {
                return _gameObjects.Values.ToList();
            }
        }
    }
}
