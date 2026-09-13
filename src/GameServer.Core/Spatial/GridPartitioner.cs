using System.Numerics;

namespace GameServer.Core.Spatial
{
    public class GridPartitioner<T> where T : IQuadTreeEntity
    {
        private readonly float _cellSize;
        private readonly Dictionary<(int, int), List<T>> _cells = new();
        private readonly Dictionary<T, (int, int)> _entityCells = new();
        private readonly object _lock = new();

        public GridPartitioner(float cellSize)
        {
            _cellSize = cellSize;
        }

        public void Insert(T entity)
        {
            var cellIndex = GetCellIndex(entity.Position);
            lock (_lock)
            {
                if (!_cells.TryGetValue(cellIndex, out var cell))
                {
                    cell = new List<T>();
                    _cells[cellIndex] = cell;
                }
                cell.Add(entity);
                _entityCells[entity] = cellIndex;
            }
        }

        public void Remove(T entity)
        {
            lock (_lock)
            {
                if (_entityCells.TryGetValue(entity, out var cellIndex))
                {
                    if (_cells.TryGetValue(cellIndex, out var cell))
                    {
                        cell.Remove(entity);
                        if (cell.Count == 0)
                        {
                            _cells.Remove(cellIndex);
                        }
                    }
                    _entityCells.Remove(entity);
                }
            }
        }

        public void Update(T entity)
        {
            lock (_lock)
            {
                if (_entityCells.TryGetValue(entity, out var oldCellIndex))
                {
                    var newCellIndex = GetCellIndex(entity.Position);
                    if (oldCellIndex == newCellIndex) return;

                    if (_cells.TryGetValue(oldCellIndex, out var oldCell))
                    {
                        oldCell.Remove(entity);
                        if (oldCell.Count == 0)
                        {
                            _cells.Remove(oldCellIndex);
                        }
                    }
                }

                var cellIndex = GetCellIndex(entity.Position);
                if (!_cells.TryGetValue(cellIndex, out var cell))
                {
                    cell = new List<T>();
                    _cells[cellIndex] = cell;
                }
                cell.Add(entity);
                _entityCells[entity] = cellIndex;
            }
        }

        public List<T> GetNearbyEntities(Vector2 position, float radius)
        {
            var results = new List<T>();
            int minCellX = (int)Math.Floor((position.X - radius) / _cellSize);
            int maxCellX = (int)Math.Floor((position.X + radius) / _cellSize);
            int minCellY = (int)Math.Floor((position.Y - radius) / _cellSize);
            int maxCellY = (int)Math.Floor((position.Y + radius) / _cellSize);

            lock (_lock)
            {
                for (int x = minCellX; x <= maxCellX; x++)
                {
                    for (int y = minCellY; y <= maxCellY; y++)
                    {
                        if (_cells.TryGetValue((x, y), out var cell))
                        {
                            results.AddRange(cell);
                        }
                    }
                }
            }

            return results;
        }

        public List<T> GetEntitiesInCell(int cellX, int cellY)
        {
            lock (_lock)
            {
                if (_cells.TryGetValue((cellX, cellY), out var cell))
                {
                    return new List<T>(cell);
                }
            }
            return new List<T>();
        }

        public void Clear()
        {
            lock (_lock)
            {
                _cells.Clear();
                _entityCells.Clear();
            }
        }

        private (int, int) GetCellIndex(Vector2 position)
        {
            int x = (int)Math.Floor(position.X / _cellSize);
            int y = (int)Math.Floor(position.Y / _cellSize);
            return (x, y);
        }
    }
}
