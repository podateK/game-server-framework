using System.Numerics;
using GameServer.Core.Spatial;

namespace GameServer.Core.Rooms
{
    public class GameObject : ISpatialEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public Vector2 Position { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Version { get; private set; }
        public bool IsDirty { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
        public DateTime LastUpdated { get; set; }

        public GameObject(string name, string type)
        {
            Name = name;
            Type = type;
            LastUpdated = DateTime.UtcNow;
        }

        public void UpdatePosition(Vector2 newPosition)
        {
            Position = newPosition;
            IsDirty = true;
            Version++;
            LastUpdated = DateTime.UtcNow;
        }

        public void SetProperty(string key, object value)
        {
            Properties[key] = value;
            IsDirty = true;
            Version++;
            LastUpdated = DateTime.UtcNow;
        }
    }
}
