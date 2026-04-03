using System;

namespace Reclaim.Grid
{
    [Serializable]
    public struct GridCoordinate : IEquatable<GridCoordinate>
    {
        public int X;
        public int Y;

        public GridCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static GridCoordinate operator +(GridCoordinate a, GridCoordinate b)
        {
            return new GridCoordinate(a.X + b.X, a.Y + b.Y);
        }

        public bool Equals(GridCoordinate other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridCoordinate other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
