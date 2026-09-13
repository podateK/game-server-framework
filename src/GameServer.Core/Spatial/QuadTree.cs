using System.Numerics;

namespace GameServer.Core.Spatial
{
    public class QuadTree<T> where T : IQuadTreeEntity
    {
        private readonly int _maxEntities;
        private readonly int _maxDepth;
        private readonly QuadTreeNode _root;

        public QuadTree(BoundingBox bounds, int maxEntities = 10, int maxDepth = 8)
        {
            _maxEntities = maxEntities;
            _maxDepth = maxDepth;
            _root = new QuadTreeNode(bounds, 0, maxEntities, maxDepth);
        }

        public void Insert(T entity)
        {
            _root.Insert(entity);
        }

        public bool Remove(T entity)
        {
            return _root.Remove(entity);
        }

        public List<T> Query(BoundingBox area)
        {
            var results = new List<T>();
            _root.Query(area, results);
            return results;
        }

        public List<T> Query(Vector2 point, float radius)
        {
            var area = new BoundingBox(
                new Vector2(point.X - radius, point.Y - radius),
                new Vector2(point.X + radius, point.Y + radius));
            return Query(area);
        }

        public void Clear()
        {
            _root.Clear();
        }

        private class QuadTreeNode
        {
            private readonly BoundingBox _bounds;
            private readonly int _depth;
            private readonly int _maxEntities;
            private readonly int _maxDepth;
            private readonly List<T> _entities = new();
            private QuadTreeNode[]? _nodes;

            public QuadTreeNode(BoundingBox bounds, int depth, int maxEntities, int maxDepth)
            {
                _bounds = bounds;
                _depth = depth;
                _maxEntities = maxEntities;
                _maxDepth = maxDepth;
            }

            public void Insert(T entity)
            {
                if (_nodes != null)
                {
                    var index = GetIndex(entity.Position);
                    if (index != -1)
                    {
                        _nodes[index].Insert(entity);
                        return;
                    }
                }

                _entities.Add(entity);

                if (_entities.Count > _maxEntities && _depth < _maxDepth && _nodes == null)
                {
                    Subdivide();
                    var toReinsert = new List<T>(_entities);
                    _entities.Clear();

                    foreach (var e in toReinsert)
                    {
                        var index = GetIndex(e.Position);
                        if (index != -1)
                        {
                            _nodes[index].Insert(e);
                        }
                        else
                        {
                            _entities.Add(e);
                        }
                    }
                }
            }

            public bool Remove(T entity)
            {
                if (_entities.Remove(entity)) return true;

                if (_nodes != null)
                {
                    var index = GetIndex(entity.Position);
                    if (index != -1)
                    {
                        return _nodes[index].Remove(entity);
                    }
                }

                return false;
            }

            public void Query(BoundingBox area, List<T> results)
            {
                if (!_bounds.Intersects(area)) return;

                foreach (var entity in _entities)
                {
                    if (area.Contains(entity.Position))
                    {
                        results.Add(entity);
                    }
                }

                if (_nodes != null)
                {
                    foreach (var node in _nodes)
                    {
                        node.Query(area, results);
                    }
                }
            }

            public void Clear()
            {
                _entities.Clear();
                if (_nodes != null)
                {
                    foreach (var node in _nodes)
                    {
                        node.Clear();
                    }
                    _nodes = null;
                }
            }

            private void Subdivide()
            {
                var mid = new Vector2(
                    (_bounds.Min.X + _bounds.Max.X) / 2,
                    (_bounds.Min.Y + _bounds.Max.Y) / 2);

                _nodes = new QuadTreeNode[4]
                {
                    new(new BoundingBox(_bounds.Min, mid), _depth + 1, _maxEntities, _maxDepth),
                    new(new BoundingBox(new Vector2(mid.X, _bounds.Min.Y), new Vector2(_bounds.Max.X, mid.Y)), _depth + 1, _maxEntities, _maxDepth),
                    new(new BoundingBox(new Vector2(_bounds.Min.X, mid.Y), new Vector2(mid.X, _bounds.Max.Y)), _depth + 1, _maxEntities, _maxDepth),
                    new(new BoundingBox(mid, _bounds.Max), _depth + 1, _maxEntities, _maxDepth)
                };
            }

            private int GetIndex(Vector2 position)
            {
                var midX = (_bounds.Min.X + _bounds.Max.X) / 2;
                var midY = (_bounds.Min.Y + _bounds.Max.Y) / 2;

                bool topHalf = position.Y < midY;
                bool leftHalf = position.X < midX;

                return (topHalf, leftHalf) switch
                {
                    (true, true) => 0,
                    (true, false) => 1,
                    (false, true) => 2,
                    (false, false) => 3
                };
            }
        }
    }
}
