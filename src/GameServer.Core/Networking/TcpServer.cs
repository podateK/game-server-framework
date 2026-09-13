using System.Net;
using System.Net.Sockets;

namespace GameServer.Core.Networking
{
    public class TcpServer
    {
        private readonly Socket _listener;
        private readonly List<ClientSession> _sessions = new();
        private readonly object _lock = new();
        private bool _isRunning;

        public event Action<ClientSession>? OnClientConnected;
        public event Action<ClientSession>? OnClientDisconnected;
        public event Action<ClientSession, ReadOnlyMemory<byte>>? OnPacketReceived;

        public TcpServer(string ip, int port)
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
        }

        public void Start(int backlog = 100)
        {
            _listener.Listen(backlog);
            _isRunning = true;
            _ = AcceptLoopAsync();
        }

        private async Task AcceptLoopAsync()
        {
            while (_isRunning)
            {
                try
                {
                    var socket = await _listener.AcceptAsync();
                    var session = new ClientSession(socket);
                    lock (_lock)
                    {
                        _sessions.Add(session);
                    }
                    OnClientConnected?.Invoke(session);
                    _ = HandleClientAsync(session);
                }
                catch
                {
                    if (!_isRunning) break;
                }
            }
        }

        private async Task HandleClientAsync(ClientSession session)
        {
            try
            {
                while (session.IsConnected)
                {
                    var result = await session.Reader.ReadAsync();
                    var buffer = result.Buffer;

                    if (buffer.IsEmpty && result.IsCompleted) break;

                    while (buffer.Length >= PacketHeader.Size)
                    {
                        Span<byte> headerBytes = stackalloc byte[PacketHeader.Size];
                        buffer.Slice(0, PacketHeader.Size).CopyTo(headerBytes);
                        ushort length = BitConverter.ToUInt16(headerBytes);

                        if (buffer.Length < length) break;

                        var packetData = buffer.Slice(0, length).ToArray();
                        OnPacketReceived?.Invoke(session, packetData);

                        buffer = buffer.Slice(length);
                    }

                    session.Reader.AdvanceTo(buffer.Start, buffer.End);
                }
            }
            catch
            {
            }
            finally
            {
                lock (_lock)
                {
                    _sessions.Remove(session);
                }
                await session.DisconnectAsync();
                OnClientDisconnected?.Invoke(session);
            }
        }

        public void Stop()
        {
            _isRunning = false;
            try
            {
                _listener.Close();
            }
            catch { }
            lock (_lock)
            {
                foreach (var session in _sessions)
                {
                    session.DisconnectAsync().Wait();
                }
                _sessions.Clear();
            }
        }
    }
}
