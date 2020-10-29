using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Foundry
{
    /// <summary>
    /// Represents a 2D position with each coordinate stored in an <see cref="int"/>.
    /// </summary>
    public readonly struct Point32 : IEquatable<Point32>
    {
        /// <summary>
        /// A <see cref="Point32"/> that has <see cref="X"/> and <see cref="Y"/> set to 0.
        /// </summary>
        public static Point32 Zero { get; } = new Point32();

        /// <summary>
        /// The X coordinate.
        /// </summary>
        public readonly int X { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        /// <summary>
        /// The Y coordinate.
        /// </summary>
        public readonly int Y { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public Point32(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Point32(Point32 other)
        {
            X = other.X;
            Y = other.Y;
        }

        public readonly Point32 Add(int offset)
            => Add(offset, offset);

        public readonly Point32 Add(int x, int y)
        {
            return new Point32(X + x, y + y);
        }

        public readonly Point32 Add(Point32 other)
        {
            return new Point32(X + other.X, Y + other.Y);
        }

        public readonly Point32 Subtract(int offset)
            => Subtract(offset, offset);

        public readonly Point32 Subtract(int x, int y)
        {
            return new Point32(X - x, Y - y);
        }

        public readonly Point32 Subtract(Point32 other)
        {
            return new Point32(X - other.X, Y - other.Y);
        }

        public readonly Point32 Multiply(int amount)
        {
            return this * amount;
        }

        public readonly Point32 Multiply(double amount)
        {
            return this * amount;
        }

        public readonly Point32 Multiply(double amount, MidpointRounding rounding)
        {
            return new Point32((int)Math.Round(X * amount, rounding), (int)Math.Round(Y * amount, rounding));
        }

        public readonly Point32 Divide(int amount)
        {
            return this / amount;
        }

        public readonly Point32 Divide(long amount)
        {
            return this / amount;
        }

        public readonly Point32 Divide(double amount)
        {
            return this / amount;
        }

        public readonly Point32 Divide(double amount, MidpointRounding rounding)
        {
            return new Point32((int)Math.Round(X / amount, rounding), (int)Math.Round(Y / amount, rounding));
        }

        public readonly void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }

        public override readonly bool Equals(object? obj)
            => obj is Point32 point && Equals(point);

        public readonly bool Equals(Point32 other)
        {
            return X == other.X &&
                   Y == other.Y;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static Point32 operator *(Point32 point, int amount)
        {
            return new Point32(point.X * amount, point.Y * amount);
        }

        public static Point32 operator *(Point32 point, double amount)
        {
            return new Point32((int)(point.X * amount), (int)(point.Y * amount));
        }

        public static Point32 operator /(Point32 point, int amount)
        {
            return new Point32(point.X / amount, point.Y / amount);
        }

        public static Point32 operator /(Point32 point, double amount)
        {
            return new Point32((int)(point.X / amount), (int)(point.Y / amount));
        }

        public static bool operator ==(Point32 left, Point32 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Point32 left, Point32 right)
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
