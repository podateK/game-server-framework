using LiteDB;

namespace GameServer.Core.Persistence
{
    public class PlayerProfile
    {
        public ObjectId Id { get; set; } = ObjectId.NewObjectId();
        public string PlayerId { get; set; } = string.Empty;
        public Dictionary<string, string> Attributes { get; set; } = new();
    }

    public class DocumentStore : IDisposable
    {
        private readonly LiteDatabase _db;

        public DocumentStore(string connectionString)
        {
            _db = new LiteDatabase(connectionString);
        }

        public ILiteCollection<PlayerProfile> Profiles => _db.GetCollection<PlayerProfile>("profiles");

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
