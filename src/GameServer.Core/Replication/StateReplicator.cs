using GameServer.Core.Rooms;
using GameServer.Core.Spatial;

namespace GameServer.Core.Replication
{
    public struct EntityStateMessage
    {
        public ulong Tick;
        public float X;
        public float Y;
    }

    public class StateReplicator
    {
        private ulong _currentTick;

        public EntityStateMessage CreateStateSnapshot(GameObject obj)
        {
            _currentTick++;
            return new EntityStateMessage
            {
                Tick = _currentTick,
                X = obj.Position.X,
                Y = obj.Position.Y
            };
        }
    }
}
