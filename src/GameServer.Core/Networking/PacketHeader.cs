namespace GameServer.Core.Networking
{
    public readonly struct PacketHeader
    {
        public const int Size = 6;
        public ushort Length { get; }
        public ushort OpCode { get; }
        public ushort Flags { get; }

        public PacketHeader(ushort length, ushort opCode, ushort flags)
        {
            Length = length;
            OpCode = opCode;
            Flags = flags;
        }
    }
}
