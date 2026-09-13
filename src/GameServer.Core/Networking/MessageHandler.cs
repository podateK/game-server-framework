using GameServer.Core.Commands;

namespace GameServer.Core.Networking
{
    public class MessageHandler
    {
        private readonly Dictionary<ushort, IPacketHandler> _handlers = new();

        public void Register(IPacketHandler handler)
        {
            _handlers[handler.OpCode] = handler;
        }

        public bool TryGetHandler(ushort opCode, out IPacketHandler handler)
        {
            return _handlers.TryGetValue(opCode, out handler!);
        }

        public IEnumerable<ushort> GetRegisteredOpCodes()
        {
            return _handlers.Keys;
        }
    }
}
