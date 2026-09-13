using GameServer.Core.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace GameServer.Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly EfCorePersistence _persistence;

        public PlayersController(EfCorePersistence persistence)
        {
            _persistence = persistence;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePlayer([FromBody] CreatePlayerRequest request)
        {
            var player = new PlayerData
            {
                Username = request.Username,
                Level = 1,
                Experience = 0
            };

            await _persistence.SavePlayerAsync(player);
            return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, player);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlayer(string id)
        {
            var player = await _persistence.GetPlayerAsync(id);
            if (player == null) return NotFound();
            return Ok(player);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlayer(string id)
        {
            await _persistence.DeletePlayerAsync(id);
            return NoContent();
        }
    }

    public class CreatePlayerRequest
    {
        public string Username { get; set; } = string.Empty;
    }
}
