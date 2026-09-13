using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameServer.Core.Persistence;

namespace GameServer.Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly GameDbContext _context;

        public RoomsController(GameDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRooms()
        {
            var rooms = await _context.Rooms.ToListAsync();
            return Ok(rooms);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
        {
            var room = new RoomData
            {
                Name = request.Name,
                MaxPlayers = request.MaxPlayers,
                HostId = request.HostId
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoom(string id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();
            return Ok(room);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(string id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }
    }

    public class CreateRoomRequest
    {
        public string Name { get; set; } = string.Empty;
        public int MaxPlayers { get; set; } = 10;
        public string HostId { get; set; } = string.Empty;
    }
}
