using System.Numerics;

namespace GameServer.Core.Spatial
{
    public class BoundingBox
    {
        public Vector2 Min { get; set; }
        public Vector2 Max { get; set; }

        public BoundingBox(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }

        public bool Contains(Vector2 point)
        {
            return point.X >= Min.X && point.X <= Max.X &&
                   point.Y >= Min.Y && point.Y <= Max.Y;
        }

        public bool Intersects(BoundingBox other)
        {
            return !(other.Min.X > Max.X || other.Max.X < Min.X ||
                     other.Min.Y > Max.Y || other.Max.Y < Min.Y);
        }
    }
}
