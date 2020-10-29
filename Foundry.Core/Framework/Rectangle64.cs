using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Foundry
{
    /// <summary>
    /// Describes a 2D rectangle.
    /// </summary>
    public readonly struct Rectangle64 : IEquatable<Rectangle64>
    {
        /// <summary>
        /// A rectangle at coordinates (0,0) with a width and height of 0.
        /// </summary>
        public static Rectangle64 Empty { get; } = new Rectangle64();

        /// <summary>
        /// The X coordinate.
        /// </summary>
        public readonly long X { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        /// <summary>
        /// The Y coordinate.
        /// </summary>
        public readonly long Y { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        /// <summary>
        /// The width.
        /// </summary>
        public readonly long Width { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        /// <summary>
        /// The height.
        /// </summary>
        public readonly long Height { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        /// <summary>
        /// The left (X) coordinate.
        /// </summary>
        public readonly long Left
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => X;
        }

        /// <summary>
        /// The top (Y) coordinate.
        /// </summary>
        public readonly long Top
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Y;
        }

        /// <summary>
        /// The right coordinate.
        /// </summary>
        public readonly long Right
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => X + Width;
        }

        /// <summary>
        /// The bottom coordinate.
        /// </summary>
        public readonly long Bottom
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Y + Height;
        }

        /// <summary>
        /// The top left coordinate.
        /// </summary>
        public readonly Point64 TopLeft => new Point64(X, Y);

        /// <summary>
        /// The top right coordinate.
        /// </summary>
        public readonly Point64 TopRight => new Point64(X + Width, Y);

        /// <summary>
        /// The bottom left coordinate.
        /// </summary>
        public readonly Point64 BottomLeft => new Point64(X, Y + Height);

        /// <summary>
        /// The bottom right coordinate.
        /// </summary>
        public readonly Point64 BottomRight => new Point64(X + Width, Y + Height);

        /// <summary>
        /// Whether this instance has an <see cref="X"/> and <see cref="Y"/> of 0 and has
        /// no <see cref="Width"/> or <see cref="Height"/>.
        /// </summary>
        public readonly bool IsEmpty => X == 0 && Y == 0 && Width == 0 && Height == 0;

        public Rectangle64(long x, long y, long width, long height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectangle64(Point64 topLeft, long width, long height)
            : this(topLeft.X, topLeft.Y, width, height)
        {

        }

        public Rectangle64(Rectangle64 other)
        {
            X = other.X;
            Y = other.Y;
            Width = other.Width;
            Height = other.Height;
        }

        /// <summary>
        /// Returns whether one <see cref="Rectangle64"/> instance
        /// partially or fully overlaps another <see cref="Rectangle64"/>
        /// instance.
        /// </summary>
        public static bool Intersects(Rectangle64 a, Rectangle64 b)
        {
            return b.Left < a.Right &&
                   a.Left < b.Right &&
                   b.Top < b.Bottom &&
                   a.Top < a.Bottom;
        }

        /// <summary>
        /// Creates a <see cref="Rectangle64"/> that represents the intersection
        /// between two <see cref="Rectangle64"/> instances.
        /// </summary>
        public static Rectangle64 CreateIntersection(Rectangle64 a, Rectangle64 b)
        {
            long x1 = Math.Max(a.Left, b.Left);
            long x2 = Math.Min(a.Right, b.Right);
            long y1 = Math.Max(a.Top, b.Top);
            long y2 = Math.Min(a.Bottom, b.Bottom);

            return x2 >= x1 && y2 >= y1
                ? new Rectangle64(x1, y1, x2 - x1, y2 - y1)
                : Empty;
        }

        /// <summary>
        /// Returns whether one <see cref="Rectangle64"/> instance fully contains
        /// another <see cref="Rectangle64"/> instance.
        /// </summary>
        public static bool Contains(Rectangle64 a, Rectangle64 b)
        {
            return b.Left <= a.Left &&
                   b.Right <= a.Right &&
                   b.Top <= a.Top &&
                   b.Bottom <= a.Bottom;
        }

        /// <summary>
        /// Returns whether this <see cref="Rectangle64"/> instance
        /// partially or fully overlaps another <see cref="Rectangle64"/>
        /// instance.
        /// </summary>
        public readonly bool Intersects(Rectangle64 other)
        {
            return Intersects(this, other);
        }

        /// <summary>
        /// Creates a <see cref="Rectangle64"/> that represents the intersection
        /// between this <see cref="Rectangle64"/> and another.
        /// </summary>
        public readonly Rectangle64 CreateIntersection(Rectangle64 other)
        {
            return CreateIntersection(this, other);
        }

        /// <summary>
        /// Returns whether this <see cref="Rectangle64"/> fully contains another
        /// <see cref="Rectangle64"/> instance.
        /// </summary>
        public readonly bool Contains(Rectangle64 other)
        {
            return Contains(this, other);
        }

        /// <summary>
        /// Returns whether this <see cref="Rectangle64"/> contains the coordinates
        /// at <paramref name="x"/> and <paramref name="y"/>.
        /// </summary>
        public readonly bool Contains(long x, long y)
        {
            return X <= Left &&
                   x < Right &&
                   Y <= Top &&
                   y < Bottom;
        }

        /// <summary>
        /// Returns whether this <see cref="Rectangle64"/> contains the specified
        /// <see cref="Point64"/>.
        /// </summary>
        public readonly bool Contains(Point64 point)
            => Contains(point.X, point.Y);

        public readonly Rectangle64 AddX(long value)
        {
            return new Rectangle64(X + value, Y, Width, Height);
        }

        public readonly Rectangle64 AddY(long value)
        {
            return new Rectangle64(X, Y + value, Width, Height);
        }

        public readonly Rectangle64 AddWidth(long value)
        {
            return new Rectangle64(X, Y, Width + value, Height);
        }

        public readonly Rectangle64 AddHeight(long value)
        {
            return new Rectangle64(X, Y, Width, Height + value);
        }

        /// <summary>
        /// Creates a new rectangle whose <see cref="X"/> and
        /// <see cref="Y"/> are the same as this instance's
        /// values offset by <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The amount to offset the <see cref="X"/> and <see cref="Y"/> value by.</param>
        public readonly Rectangle64 Offset(long value)
        {
            return new Rectangle64(X + value, Y + value, Width, Height);
        }

        /// <summary>
        /// Creates a new rectangle whose <see cref="X"/> and
        /// <see cref="Y"/> are the same as this instance's
        /// values offset by <paramref name="value"/>.
        /// </summary>
        /// <param name="x">The amount to offset the <see cref="X"/> value by.</param>
        /// <param name="y">The amount to offset the <see cref="Y"/> value by.</param>
        public readonly Rectangle64 Offset(long x, long y)
        {
            return new Rectangle64(X + x, Y + y, Width, Height);
        }

        public readonly void Deconstruct(out long x, out long y, out long width, out long height)
        {
            x = X;
            y = Y;
            width = Width;
            height = Height;
        }

        public override readonly bool Equals(object obj)
            => obj is Rectangle64 other && Equals(other);

        public readonly bool Equals(Rectangle64 other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Width == other.Width &&
                   Height == other.Height;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y, Width, Height);
        }

        public static Rectangle64 operator *(Rectangle64 rectangle, double amount)
        {
            return new Rectangle64(rectangle.X, rectangle.Y, (long)(rectangle.Width * amount), (long)(rectangle.Height * amount));
        }

        public static Rectangle64 operator /(Rectangle64 rectangle, double amount)
        {
            return new Rectangle64(rectangle.X, rectangle.Y, (long)(rectangle.Width / amount), (long)(rectangle.Height / amount));
        }

        public static bool operator ==(Rectangle64 left, Rectangle64 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Rectangle64 left, Rectangle64 right)
        {
            return !(left == right);
        }

        public override readonly string ToString()
        {
            return new StringBuilder()
                .Append('{')
                .Append("X=").Append(X.ToString(CultureInfo.CurrentCulture)).Append(", ")
                .Append("Y=").Append(Y.ToString(CultureInfo.CurrentCulture)).Append(", ")
                .Append("Width=").Append(Width.ToString(CultureInfo.CurrentCulture)).Append(", ")
                .Append("Height=").Append(Height.ToString(CultureInfo.CurrentCulture))
                .Append("}")
                .ToString();
        }
    }
}
