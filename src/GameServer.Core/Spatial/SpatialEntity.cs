using System.Numerics;

namespace GameServer.Core.Spatial
{
    public abstract class SpatialEntity : ISpatialEntity, IQuadTreeEntity
    {
        public string Id { get; init; }
        public Vector2 Position { get; set; }

        protected SpatialEntity(string id)
        {
            Id = id;
        }
    }
}
