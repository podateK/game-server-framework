using System.Numerics;

namespace GameServer.Core.Persistence
{
    public class PlayerData
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Username { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
        public int Experience { get; set; }
        public float Health { get; set; } = 100f;
        public float Mana { get; set; } = 100f;
        public string InventoryJson { get; set; } = "{}";
        public DateTime LastLogin { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class RoomData
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public int MaxPlayers { get; set; } = 10;
        public string HostId { get; set; } = string.Empty;
        public string StateJson { get; set; } = "{}";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }

    public class GameStateData
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string EntityId { get; set; } = string.Empty;
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public float RotationX { get; set; }
        public float RotationY { get; set; }
        public float RotationZ { get; set; }
        public float RotationW { get; set; } = 1f;
        public string CustomDataJson { get; set; } = "{}";
        public int Version { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
