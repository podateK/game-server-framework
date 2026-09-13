using Microsoft.EntityFrameworkCore;

namespace GameServer.Core.Persistence
{
    public class PlayerRecord
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public int Score { get; set; }
    }

    public class GameDbContext : DbContext
    {
        public DbSet<PlayerRecord> Players => Set<PlayerRecord>();

        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PlayerRecord>().HasKey(p => p.Id);
        }
    }
}
