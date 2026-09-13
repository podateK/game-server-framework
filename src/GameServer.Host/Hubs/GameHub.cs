using Microsoft.AspNetCore.SignalR;

namespace GameServer.Host.Hubs
{
    public class GameHub : Hub
    {
        public async Task JoinRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("PlayerJoined", Context.ConnectionId);
        }

        public async Task LeaveRoom(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("PlayerLeft", Context.ConnectionId);
        }

        public async Task SendMove(string roomId, float x, float y)
        {
            await Clients.Group(roomId).SendAsync("PlayerMoved", Context.ConnectionId, x, y);
        }
    }
}
