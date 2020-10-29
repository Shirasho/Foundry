using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Foundry.Collections
{
    /// <summary>
    /// A <see cref="List{T}"/> that is allocated on the stack.
    /// </summary>
    /// <typeparam name="T">The underlying type.</typeparam>
    public ref struct StackList<T>
    {
        /// <summary>
        /// The number of elements in this list.
        /// </summary>
        public int Length { get; private set; }

        private Span<T> Span;
        private T[]? ArrayFromPool;

        public ref T this[int index] => ref Span[index];

        public StackList(int initialSize)
        {
            Span = ArrayFromPool = ArrayPool<T>.Shared.Rent(initialSize);
            Length = 0;
        }

        public StackList(Span<T> initialSpan)
        {
            Span = initialSpan;
            ArrayFromPool = null;
            Length = 0;
        }

        /// <summary>
        /// Releases the underlying memory used for this <see cref="StackList{T}"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (ArrayFromPool != null)
            {
                ArrayPool<T>.Shared.Return(ArrayFromPool);
                ArrayFromPool = null;
            }
        }

        /// <summary>
        /// Appends an item to this list.
        /// </summary>
        /// <param name="item">The item to append to the list.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(T item)
        {
            int pos = Length;
            if (pos >= Span.Length)
            {
                Grow();
            }

            Span[pos] = item;
            Length = pos + 1;
        }

        /// <summary>
        /// Returns a <see cref="ReadOnlySpan{T}"/> over the elements
        /// in this list.
        /// </summary>
        public ReadOnlySpan<T> AsSpan()
        {
            return Span;
        }

        private void Grow()
        {
            T[] array = ArrayPool<T>.Shared.Rent(Span.Length * 2);
            if (!Span.TryCopyTo(array))
            {
                throw new InvalidOperationException("Unable to copy span to grown ArrayPool array.");
            }

            T[]? toReturn = ArrayFromPool;
            Span = ArrayFromPool = array;
            if (toReturn != null)
            {
                ArrayPool<T>.Shared.Return(toReturn);
            }
        }
    }
}
