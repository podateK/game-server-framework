using Microsoft.EntityFrameworkCore;

namespace GameServer.Core.Persistence
{
    public class EfCorePersistence : IDisposable
    {
        private readonly GameDbContext _context;

        public EfCorePersistence(string connectionString = "Data Source=gameserver.db")
        {
            var optionsBuilder = new DbContextOptionsBuilder<GameDbContext>();
            optionsBuilder.UseSqlite(connectionString);
            _context = new GameDbContext(optionsBuilder.Options);
            _context.Database.EnsureCreated();
        }

        public async Task SavePlayerAsync(PlayerData player)
        {
            var existing = await _context.Players.FirstOrDefaultAsync(p => p.Id == player.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(player);
            }
            else
            {
                await _context.Players.AddAsync(player);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<PlayerData?> GetPlayerAsync(string playerId)
        {
            return await _context.Players.FindAsync(playerId);
        }

        public async Task<PlayerData?> GetPlayerByUsernameAsync(string username)
        {
            return await _context.Players.FirstOrDefaultAsync(p => p.Username == username);
        }

        public async Task DeletePlayerAsync(string playerId)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player != null)
            {
                _context.Players.Remove(player);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveRoomAsync(RoomData room)
        {
            var existing = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == room.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(room);
            }
            else
            {
                await _context.Rooms.AddAsync(room);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<RoomData?> GetRoomAsync(string roomId)
        {
            return await _context.Rooms.FindAsync(roomId);
        }

        public async Task DeleteRoomAsync(string roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveGameStateAsync(GameStateData gameState)
        {
            var existing = await _context.GameStates.FirstOrDefaultAsync(g => g.Id == gameState.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(gameState);
            }
            else
            {
                await _context.GameStates.AddAsync(gameState);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<GameStateData?> GetGameStateAsync(Guid gameStateId)
        {
            return await _context.GameStates.FindAsync(gameStateId);
        }

        public async Task DeleteGameStateAsync(Guid gameStateId)
        {
            var gameState = await _context.GameStates.FindAsync(gameStateId);
            if (gameState != null)
            {
                _context.GameStates.Remove(gameState);
                await _context.SaveChangesAsync();
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
