using GameServer.Core.Rooms;
using Microsoft.AspNetCore.Mvc;

namespace GameServer.Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServerController : ControllerBase
    {
        private readonly RoomManager _roomManager;

        public ServerController(RoomManager roomManager)
        {
            _roomManager = roomManager;
        }

        [HttpGet("rooms")]
        public IActionResult GetRooms()
        {
            var rooms = _roomManager.GetAllRooms().Select(r => new { r.Id, r.Name });
            return Ok(rooms);
        }

        [HttpPost("rooms")]
        public IActionResult CreateRoom([FromBody] CreateRoomRequest request)
        {
            var room = _roomManager.CreateRoom(request.Name);
            return Ok(new { room.Id, room.Name });
        }
    }

    public class CreateRoomRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}
