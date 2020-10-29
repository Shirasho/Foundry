using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Foundry
{
    /// <summary>
    /// Describes a 2D rectangle.
    /// </summary>
    public readonly struct Rectangle32 : IEquatable<Rectangle32>
    {
        /// <summary>
        /// A rectangle at coordinates (0,0) with a width and height of 0.
        /// </summary>
        public static Rectangle32 Empty { get; } = new Rectangle32();

        /// <summary>
        /// The X coordinate.
        /// </summary>
        public readonly int X { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        /// <summary>
        /// The Y coordinate.
        /// </summary>
        public readonly int Y { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        /// <summary>
        /// The width.
        /// </summary>
        public readonly int Width { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        /// <summary>
        /// The height.
        /// </summary>
        public readonly int Height { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        /// <summary>
        /// The left (X) coordinate.
        /// </summary>
        public readonly int Left
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => X;
        }

        /// <summary>
        /// The top (Y) coordinate.
        /// </summary>
        public readonly int Top
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Y;
        }

        /// <summary>
        /// The right coordinate.
        /// </summary>
        public readonly int Right
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => X + Width;
        }

        /// <summary>
        /// The bottom coordinate.
        /// </summary>
        public readonly int Bottom
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Y + Height;
        }

        /// <summary>
        /// The top left coordinate.
        /// </summary>
        public readonly Point32 TopLeft => new Point32(X, Y);

        /// <summary>
        /// The top right coordinate.
        /// </summary>
        public readonly Point32 TopRight => new Point32(X + Width, Y);

        /// <summary>
        /// The bottom left coordinate.
        /// </summary>
        public readonly Point32 BottomLeft => new Point32(X, Y + Height);

        /// <summary>
        /// The bottom right coordinate.
        /// </summary>
        public readonly Point32 BottomRight => new Point32(X + Width, Y + Height);

        /// <summary>
        /// Whether this instance has an <see cref="X"/> and <see cref="Y"/> of 0 and has
        /// no <see cref="Width"/> or <see cref="Height"/>.
        /// </summary>
        public readonly bool IsEmpty => X == 0 && Y == 0 && Width == 0 && Height == 0;

        public Rectangle32(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectangle32(Point32 topLeft, int width, int height)
            : this(topLeft.X, topLeft.Y, width, height)
        {

        }

        public Rectangle32(Rectangle32 other)
        {
            X = other.X;
            Y = other.Y;
            Width = other.Width;
            Height = other.Height;
        }

        /// <summary>
        /// Returns whether one <see cref="Rectangle32"/> instance
        /// partially or fully overlaps another <see cref="Rectangle32"/>
        /// instance.
        /// </summary>
        public static bool Intersects(Rectangle32 a, Rectangle32 b)
        {
            return b.Left < a.Right &&
                   a.Left < b.Right &&
                   b.Top < b.Bottom &&
                   a.Top < a.Bottom;
        }

        /// <summary>
        /// Creates a <see cref="Rectangle32"/> that represents the intersection
        /// between two <see cref="Rectangle32"/> instances.
        /// </summary>
        public static Rectangle32 CreateIntersection(Rectangle32 a, Rectangle32 b)
        {
            int x1 = Math.Max(a.Left, b.Left);
            int x2 = Math.Min(a.Right, b.Right);
            int y1 = Math.Max(a.Top, b.Top);
            int y2 = Math.Min(a.Bottom, b.Bottom);

            return x2 >= x1 && y2 >= y1
                ? new Rectangle32(x1, y1, x2 - x1, y2 - y1)
                : Empty;
        }

        /// <summary>
        /// Returns whether one <see cref="Rectangle32"/> instance fully contains
        /// another <see cref="Rectangle32"/> instance.
        /// </summary>
        public static bool Contains(Rectangle32 a, Rectangle32 b)
        {
            return b.Left <= a.Left &&
                   b.Right <= a.Right &&
                   b.Top <= a.Top &&
                   b.Bottom <= a.Bottom;
        }

        /// <summary>
        /// Returns whether this <see cref="Rectangle32"/> instance
        /// partially or fully overlaps another <see cref="Rectangle32"/>
        /// instance.
        /// </summary>
        public readonly bool Intersects(Rectangle32 other)
        {
            return Intersects(this, other);
        }

        /// <summary>
        /// Creates a <see cref="Rectangle32"/> that represents the intersection
        /// between this <see cref="Rectangle32"/> and another.
        /// </summary>
        public readonly Rectangle32 CreateIntersection(Rectangle32 other)
        {
            return CreateIntersection(this, other);
        }

        /// <summary>
        /// Returns whether this <see cref="Rectangle32"/> fully contains another
        /// <see cref="Rectangle32"/> instance.
        /// </summary>
        public readonly bool Contains(Rectangle32 other)
        {
            return Contains(this, other);
        }

        /// <summary>
        /// Returns whether this <see cref="Rectangle32"/> contains the coordinates
        /// at <paramref name="x"/> and <paramref name="y"/>.
        /// </summary>
        public readonly bool Contains(int x, int y)
        {
            return X <= Left &&
                   x < Right &&
                   Y <= Top &&
                   y < Bottom;
        }

        /// <summary>
        /// Returns whether this <see cref="Rectangle32"/> contains the specified
        /// <see cref="Point32"/>.
        /// </summary>
        public readonly bool Contains(Point32 point)
            => Contains(point.X, point.Y);

        public readonly Rectangle32 AddX(int value)
        {
            return new Rectangle32(X + value, Y, Width, Height);
        }

        public readonly Rectangle32 AddY(int value)
        {
            return new Rectangle32(X, Y + value, Width, Height);
        }

        public readonly Rectangle32 AddWidth(int value)
        {
            return new Rectangle32(X, Y, Width + value, Height);
        }

        public readonly Rectangle32 AddHeight(int value)
        {
            return new Rectangle32(X, Y, Width, Height + value);
        }

        /// <summary>
        /// Creates a new rectangle whose <see cref="X"/> and
        /// <see cref="Y"/> are the same as this instance's
        /// values offset by <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The amount to offset the <see cref="X"/> and <see cref="Y"/> value by.</param>
        public readonly Rectangle32 Offset(int value)
        {
            return new Rectangle32(X + value, Y + value, Width, Height);
        }

        /// <summary>
        /// Creates a new rectangle whose <see cref="X"/> and
        /// <see cref="Y"/> are the same as this instance's
        /// values offset by <paramref name="value"/>.
        /// </summary>
        /// <param name="x">The amount to offset the <see cref="X"/> value by.</param>
        /// <param name="y">The amount to offset the <see cref="Y"/> value by.</param>
        public readonly Rectangle32 Offset(int x, int y)
        {
            return new Rectangle32(X + x, Y + y, Width, Height);
        }

        public readonly void Deconstruct(out int x, out int y, out int width, out int height)
        {
            x = X;
            y = Y;
            width = Width;
            height = Height;
        }

        public override readonly bool Equals(object obj)
            => obj is Rectangle32 other && Equals(other);

        public readonly bool Equals(Rectangle32 other)
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

        public static Rectangle32 operator *(Rectangle32 rectangle, double amount)
        {
            return new Rectangle32(rectangle.X, rectangle.Y, (int)(rectangle.Width * amount), (int)(rectangle.Height * amount));
        }

        public static Rectangle32 operator /(Rectangle32 rectangle, double amount)
        {
            return new Rectangle32(rectangle.X, rectangle.Y, (int)(rectangle.Width / amount), (int)(rectangle.Height / amount));
        }

        public static bool operator ==(Rectangle32 left, Rectangle32 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Rectangle32 left, Rectangle32 right)
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
