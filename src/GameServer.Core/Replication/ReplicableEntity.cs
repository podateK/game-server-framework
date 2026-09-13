using System.Numerics;

namespace GameServer.Core.Replication
{
    public class ReplicableEntity
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; } = Quaternion.Identity;
        public Vector3 Velocity { get; set; }
        public int Version { get; private set; }
        public bool IsDirty { get; private set; }
        public bool IsDestroyed { get; private set; }

        private readonly Dictionary<string, object> _properties = new();
        private readonly HashSet<string> _dirtyProperties = new();
        private readonly object _lock = new();

        public T? GetProperty<T>(string key)
        {
            lock (_lock)
            {
                if (_properties.TryGetValue(key, out var value))
                {
                    return (T)value;
                }
            }
            return default;
        }

        public void SetProperty<T>(string key, T value)
        {
            lock (_lock)
            {
                _properties[key] = value!;
                _dirtyProperties.Add(key);
                IsDirty = true;
            }
        }

        public void MarkDirty()
        {
            lock (_lock)
            {
                IsDirty = true;
            }
        }

        public void ClearDirty()
        {
            lock (_lock)
            {
                IsDirty = false;
                _dirtyProperties.Clear();
                Version++;
            }
        }

        public void Destroy()
        {
            lock (_lock)
            {
                IsDestroyed = true;
                IsDirty = true;
            }
        }

        public Dictionary<string, object> GetAllProperties()
        {
            lock (_lock)
            {
                return new Dictionary<string, object>(_properties);
            }
        }

        public HashSet<string> GetDirtyProperties()
        {
            lock (_lock)
            {
                return new HashSet<string>(_dirtyProperties);
            }
        }
    }
}
