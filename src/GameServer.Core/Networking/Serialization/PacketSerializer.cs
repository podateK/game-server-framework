using System.Buffers;
using System.Runtime.InteropServices;

namespace GameServer.Core.Networking.Serialization
{
    public static class PacketSerializer
    {
        public static unsafe void Serialize<T>(T data, ushort opCode, IBufferWriter<byte> writer) where T : unmanaged
        {
            int dataSize = sizeof(T);
            int totalSize = PacketHeader.Size + dataSize;
            
            Span<byte> headerSpan = writer.GetSpan(PacketHeader.Size);
            MemoryMarshal.Write(headerSpan, in totalSize);
            MemoryMarshal.Write(headerSpan[2..], in opCode);
            ushort flags = 0;
            MemoryMarshal.Write(headerSpan[4..], in flags);
            writer.Advance(PacketHeader.Size);

            Span<byte> dataSpan = writer.GetSpan(dataSize);
            MemoryMarshal.Write(dataSpan, in data);
            writer.Advance(dataSize);
        }

        public static unsafe bool TryDeserialize<T>(ReadOnlySequence<byte> buffer, out T data, out PacketHeader header, out long consumedBytes) where T : unmanaged
        {
            data = default;
            header = default;
            consumedBytes = 0;

            if (buffer.Length < PacketHeader.Size)
            {
                return false;
            }

            Span<byte> headerBytes = stackalloc byte[PacketHeader.Size];
            buffer.Slice(0, PacketHeader.Size).CopyTo(headerBytes);

            ushort length = MemoryMarshal.Read<ushort>(headerBytes);
            ushort opCode = MemoryMarshal.Read<ushort>(headerBytes[2..]);
            ushort flags = MemoryMarshal.Read<ushort>(headerBytes[4..]);

            header = new PacketHeader(length, opCode, flags);

            if (buffer.Length < length)
            {
                return false;
            }

            int dataSize = sizeof(T);
            Span<byte> dataBytes = stackalloc byte[dataSize];
            buffer.Slice(PacketHeader.Size, dataSize).CopyTo(dataBytes);

            data = MemoryMarshal.Read<T>(dataBytes);
            consumedBytes = length;
            return true;
        }
    }
}
