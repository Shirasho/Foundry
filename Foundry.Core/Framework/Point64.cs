using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Foundry
{
    /// <summary>
    /// Represents a 2D position with each coordinate stored in a <see cref="long"/>.
    /// </summary>
    public readonly struct Point64 : IEquatable<Point64>, IEquatable<Point32>
    {
        /// <summary>
        /// A <see cref="Point64"/> that has <see cref="X"/> and <see cref="Y"/> set to 0.
        /// </summary>
        public static Point64 Zero { get; } = new Point64();

        /// <summary>
        /// The X coordinate.
        /// </summary>
        public readonly long X { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        /// <summary>
        /// The Y coordinate.
        /// </summary>
        public readonly long Y { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public Point64(long x, long y)
        {
            X = x;
            Y = y;
        }

        public Point64(Point32 other)
        {
            X = other.X;
            Y = other.Y;
        }

        public Point64(Point64 other)
        {
            X = other.X;
            Y = other.Y;
        }

        public readonly Point64 Add(int offset)
            => Add(offset, offset);

        public readonly Point64 Add(long offset)
            => Add(offset, offset);

        public readonly Point64 Add(int x, int y)
        {
            return new Point64(X + x, y + y);
        }

        public readonly Point64 Add(long x, long y)
        {
            return new Point64(X + x, Y + y);
        }

        public readonly Point64 Add(Point32 other)
        {
            return new Point64(X + other.X, Y + other.Y);
        }

        public readonly Point64 Add(Point64 other)
        {
            return new Point64(X + other.X, Y + other.Y);
        }

        public readonly Point64 Subtract(int offset)
            => Subtract(offset, offset);

        public readonly Point64 Subtract(long offset)
            => Subtract(offset, offset);

        public readonly Point64 Subtract(int x, int y)
        {
            return new Point64(X - x, Y - y);
        }

        public readonly Point64 Subtract(long x, long y)
        {
            return new Point64(X - x, Y - y);
        }

        public readonly Point64 Subtract(Point32 other)
        {
            return new Point64(X - other.X, Y - other.Y);
        }

        public readonly Point64 Subtract(Point64 other)
        {
            return new Point64(X - other.X, Y - other.Y);
        }

        public readonly Point64 Multiply(int amount)
        {
            return this * amount;
        }

        public readonly Point64 Multiply(long amount)
        {
            return this * amount;
        }

        public readonly Point64 Multiply(double amount)
        {
            return this * amount;
        }

        public readonly Point64 Multiply(double amount, MidpointRounding rounding)
        {
            return new Point64((long)Math.Round(X * amount, rounding), (long)Math.Round(Y * amount, rounding));
        }

        public readonly Point64 Divide(int amount)
        {
            return this / amount;
        }

        public readonly Point64 Divide(long amount)
        {
            return this / amount;
        }

        public readonly Point64 Divide(double amount)
        {
            return this / amount;
        }

        public readonly Point64 Divide(double amount, MidpointRounding rounding)
        {
            return new Point64((long)Math.Round(X / amount, rounding), (long)Math.Round(Y / amount, rounding));
        }

        public readonly void Deconstruct(out long x, out long y)
        {
            x = X;
            y = Y;
        }

        public override readonly bool Equals(object? obj)
            => obj is Point64 point && Equals(point);

        public readonly bool Equals(Point32 other)
        {
            return X == other.X &&
                   Y == other.Y;
        }

        public readonly bool Equals(Point64 other)
        {
            return X == other.X &&
                   Y == other.Y;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static Point64 operator *(Point64 point, int amount)
        {
            return new Point64(point.X * amount, point.Y * amount);
        }

        public static Point64 operator *(Point64 point, long amount)
        {
            return new Point64(point.X * amount, point.Y * amount);
        }

        public static Point64 operator *(Point64 point, double amount)
        {
            return new Point64((long)(point.X * amount), (long)(point.Y * amount));
        }

        public static Point64 operator /(Point64 point, int amount)
        {
            return new Point64(point.X / amount, point.Y / amount);
        }

        public static Point64 operator /(Point64 point, long amount)
        {
            return new Point64(point.X / amount, point.Y / amount);
        }

        public static Point64 operator /(Point64 point, double amount)
        {
            return new Point64((long)(point.X / amount), (long)(point.Y / amount));
        }

        public static bool operator ==(Point64 left, Point32 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Point64 left, Point32 right)
        {
            return !(left == right);
        }

        public static bool operator ==(Point32 left, Point64 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Point32 left, Point64 right)
        {
            return !(left == right);
        }

        public static bool operator ==(Point64 left, Point64 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Point64 left, Point64 right)
        {
            return !(left == right);
        }

        public override readonly string ToString()
        {
            return "{" +
                $"X={X.ToString(CultureInfo.CurrentCulture)}, " +
                $"Y={Y.ToString(CultureInfo.CurrentCulture)}" +
                "}";
        }
    }
}
