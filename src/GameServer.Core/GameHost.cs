using GameServer.Core.Rooms;
using GameServer.Core.Replication;
using GameServer.Core.Spatial;

namespace GameServer.Core
{
    public class GameHost
    {
        private readonly TcpServer _tcpServer;
        private readonly RoomManager _roomManager;
        private readonly StateReplicator _stateReplicator;
        private readonly MessageHandler _messageHandler;

        public TcpServer TcpServer => _tcpServer;
        public RoomManager RoomManager => _roomManager;
        public StateReplicator StateReplicator => _stateReplicator;
        public MessageHandler MessageHandler => _messageHandler;

        public GameHost(string ip, int port)
        {
            _tcpServer = new TcpServer(ip, port);
            _roomManager = new RoomManager();
            _stateReplicator = new StateReplicator();
            _messageHandler = new MessageHandler();

            _tcpServer.OnClientConnected += OnClientConnected;
            _tcpServer.OnClientDisconnected += OnClientDisconnected;
            _tcpServer.OnPacketReceived += OnPacketReceived;
        }

        private void OnClientConnected(ClientSession session)
        {
            _stateReplicator.RegisterConnection(session);
        }

        private void OnClientDisconnected(ClientSession session)
        {
            _stateReplicator.UnregisterConnection(session);
            _roomManager.RemoveClientFromAllRooms(session.Id);
        }

        private async void OnPacketReceived(ClientSession session, ReadOnlyMemory<byte> packet)
        {
            if (packet.Length < 6) return;

            var length = BitConverter.ToUInt16(packet.Span);
            var opCode = BitConverter.ToUInt16(packet.Span[2..]);
            var payload = packet[6..length];

            if (_messageHandler.TryGetHandler(opCode, out var handler))
            {
                await handler.HandleAsync(session, payload);
            }
        }

        public void Start()
        {
            _tcpServer.Start();
        }

        public void Stop()
        {
            _tcpServer.Stop();
        }
    }
}
