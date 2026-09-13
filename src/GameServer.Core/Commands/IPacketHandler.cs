using System.Buffers;
using GameServer.Core.Networking;

namespace GameServer.Core.Commands
{
    public interface IPacketHandler
    {
        ushort OpCode { get; }
        ValueTask HandleAsync(GameConnection connection, ReadOnlySequence<byte> payload, CancellationToken cancellationToken);
    }
}
