using System.IO.Pipelines;
using System.Net.Sockets;

namespace GameServer.Core.Networking
{
    public class ClientSession
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public Socket Socket { get; }
        public PipeReader Reader { get; }
        public PipeWriter Writer { get; }
        public bool IsConnected { get; private set; } = true;

        public ClientSession(Socket socket)
        {
            Socket = socket;
            var networkStream = new NetworkStream(socket, false);
            Reader = PipeReader.Create(networkStream);
            Writer = PipeWriter.Create(networkStream);
        }

        public async Task DisconnectAsync()
        {
            if (!IsConnected) return;
            IsConnected = false;
            try
            {
                Socket.Shutdown(SocketShutdown.Both);
            }
            catch { }
            Socket.Close();
            await Reader.CompleteAsync();
            await Writer.CompleteAsync();
        }
    }
}
