using System;

namespace Foundry.Collections
{
    /// <summary>
    /// A wrapper around a <see cref="Span{T}"/> that treats it like a 2D array.
    /// </summary>
    /// <typeparam name="T">The underlying type contained within the <see cref="Span{T}"/>.</typeparam>
    public ref struct Array2D<T>
    {
        /// <summary>
        /// The width of the 2D array.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// The height of the 2D array.
        /// </summary>
        public int Height { get; }

        private readonly Span<T> UnderlyingArray;

        public Array2D(Span<T> span, int width, int height)
        {
            if (span.Length != width * height)
            {
                throw new ArgumentException("Span length does not equal width * height.");
            }

            UnderlyingArray = span;
            Width = width;
            Height = height;
        }

        public T this[int x, int y]
        {
            get
            {
                return UnderlyingArray[y * Width + x];
            }
            set
            {
                UnderlyingArray[y * Width + x] = value;
            }
        }
    }
}
