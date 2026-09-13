using System.Collections.Concurrent;

namespace GameServer.Core.Spatial
{
    public interface ISpatialEntity
    {
        string Id { get; }
        Vector2 Position { get; }
    }

    public class SpatialGrid<T> where T : ISpatialEntity
    {
        private readonly float _cellSize;
        private readonly ConcurrentDictionary<(int, int), HashSet<T>> _grid = new();

        public SpatialGrid(float cellSize)
        {
            _cellSize = cellSize;
        }

        private (int, int) GetCell(Vector2 position)
        {
            int x = (int)Math.Floor(position.X / _cellSize);
            int y = (int)Math.Floor(position.Y / _cellSize);
            return (x, y);
        }

        public void AddOrUpdate(T entity)
        {
            Remove(entity);
            var cell = GetCell(entity.Position);
            _grid.AddOrUpdate(cell, 
                _ => new HashSet<T> { entity }, 
                (_, set) => { lock (set) { set.Add(entity); } return set; });
        }

        public void Remove(T entity)
        {
            foreach (var kvp in _grid)
            {
                lock (kvp.Value)
                {
                    kvp.Value.Remove(entity);
                }
            }
        }

        public List<T> GetNearby(Vector2 position, float radius)
        {
            var results = new List<T>();
            int cellRadius = (int)Math.Ceiling(radius / _cellSize);
            var centerCell = GetCell(position);

            for (int dx = -cellRadius; dx <= cellRadius; dx++)
            {
                for (int dy = -cellRadius; dy <= cellRadius; dy++)
                {
                    var cellKey = (centerCell.Item1 + dx, centerCell.Item2 + dy);
                    if (_grid.TryGetValue(cellKey, out var set))
                    {
                        lock (set)
                        {
                            foreach (var entity in set)
                            {
                                if (Vector2.DistanceSquared(entity.Position, position) <= radius * radius)
                                {
                                    results.Add(entity);
                                }
                            }
                        }
                    }
                }
            }

            return results;
        }
    }
}
