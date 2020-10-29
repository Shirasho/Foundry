using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Foundry
{
    /// <summary>
    /// A utility class to sort two elements.
    /// </summary>
    public abstract class OrderedElements
    {
        /// <inheritdoc />
        private protected OrderedElements() { }

        /// <summary>
        /// Creates a new <see cref="OrderedElements{T}"/> using the two provided elements.
        /// </summary>
        /// <typeparam name="T">The type of the comparable.</typeparam>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static OrderedElements<T> Create<T>([AllowNull] T item1, [AllowNull] T item2)
            where T : IComparable<T> => Create(item1, item2, (IComparer<T>?)null);

        /// <summary>
        /// Creates a new <see cref="OrderedElements{T}"/> using the two provided elements.
        /// </summary>
        /// <typeparam name="T">The type of the comparable.</typeparam>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static StructOrderedElements<T> Create<T>(T? item1, T? item2)
            where T : struct, IComparable<T> => Create(item1, item2, (IComparer<T?>?)null);

        /// <summary>
        /// Creates a new <see cref="OrderedElements{T}"/> using the two provided elements.
        /// </summary>
        /// <typeparam name="T">The type of the comparable.</typeparam>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        /// <param name="comparer">The comparer to use for sorting.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static OrderedElements<T> Create<T>([AllowNull] T item1, [AllowNull] T item2, IComparer<T>? comparer)
            where T : IComparable<T>
        {
            var comp = comparer ?? Comparer<T>.Default;
            return Create(item1, item2, new Func<T, T, int>(comp.Compare));
        }

        /// <summary>
        /// Creates a new <see cref="OrderedElements{T}"/> using the two provided elements.
        /// </summary>
        /// <typeparam name="T">The type of the comparable.</typeparam>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        /// <param name="comparer">The comparer to use for sorting.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static StructOrderedElements<T> Create<T>(T? item1, T? item2, IComparer<T?>? comparer)
            where T : struct, IComparable<T>
        {
            var comp = comparer ?? Comparer<T?>.Default;
            return Create(item1, item2, new Func<T?, T?, int>(comp.Compare));
        }

        /// <summary>
        /// Creates a new <see cref="OrderedElements{T}"/> using the two provided elements.
        /// </summary>
        /// <typeparam name="T">The type of the comparable.</typeparam>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        /// <param name="comparer">The comparer to use for sorting.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static OrderedElements<T> Create<T>([AllowNull] T item1, [AllowNull] T item2, Func<T, T, int>? comparer)
            where T : IComparable<T> => new OrderedElements<T>(item1, item2, comparer);

        /// <summary>
        /// Creates a new <see cref="OrderedElements{T}"/> using the two provided elements.
        /// </summary>
        /// <typeparam name="T">The type of the comparable.</typeparam>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        /// <param name="comparer">The comparer to use for sorting.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static StructOrderedElements<T> Create<T>(T? item1, T? item2, Func<T?, T?, int>? comparer)
            where T : struct, IComparable<T> => new StructOrderedElements<T>(item1, item2, comparer);

        /// <summary>
        /// Gets the lesser element.
        /// </summary>
        public abstract object? GetLesser();

        /// <summary>
        /// Gets the greater element.
        /// </summary>
        public abstract object? GetGreater();
    }

    /// <summary>
    /// A utility class to sort two elements.
    /// </summary>
    public class OrderedElements<T> : OrderedElements
        where T : IComparable<T>
    {
        /// <summary>
        /// The lesser of the two elements.
        /// </summary>
        [AllowNull, MaybeNull]
        public T Lesser { get; }

        /// <summary>
        /// The greater of the two elements.
        /// </summary>
        [AllowNull, MaybeNull]
        public T Greater { get; }

        /// <summary>
        /// Creates a new <see cref="OrderedElements{T}"/> using the two provided elements.
        /// </summary>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public OrderedElements([AllowNull] T item1, [AllowNull] T item2)
            : this(item1, item2, (IComparer<T>?)null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="OrderedElements{T}"/> using the two provided elements.
        /// </summary>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        /// <param name="comparer">The comparer to use for sorting.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public OrderedElements([AllowNull] T item1, [AllowNull] T item2, IComparer<T>? comparer)
            : this(item1, item2, comparer != null ? comparer.Compare : (Func<T, T, int>?)null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="OrderedElements{T}"/> using the two provided elements.
        /// </summary>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        /// <param name="comparer">The comparer func to use for sorting.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public OrderedElements([AllowNull] T item1, [AllowNull] T item2, Func<T, T, int>? comparer)
        {
            comparer ??= Comparer<T>.Default.Compare;
            Lesser = comparer(item1!, item2!) < 0 ? item1 : item2;
            Greater = comparer(item1!, item2!) >= 0 ? item1 : item2;
        }

        /// <summary>
        /// Deconstructs the object.
        /// </summary>
        /// <param name="lesser"></param>
        /// <param name="greater"></param>
        public void Deconstruct([MaybeNull] out T lesser, [MaybeNull] out T greater)
        {
            lesser = Lesser;
            greater = Greater;
        }

        /// <summary>
        /// Gets the greater element.
        /// </summary>
        public override object? GetGreater() => Greater;

        /// <summary>
        /// Gets the lesser element.
        /// </summary>
        public override object? GetLesser() => Lesser;
    }

    /// <summary>
    /// A utility class to sort two elements.
    /// </summary>
    public class StructOrderedElements<T> : OrderedElements
        where T : struct, IComparable<T>
    {
        /// <summary>
        /// The lesser of the two elements.
        /// </summary>
        public T? Lesser { get; }

        /// <summary>
        /// The greater of the two elements.
        /// </summary>
        public T? Greater { get; }

        /// <summary>
        /// Creates a new <see cref="StructOrderedElements{T}"/> using the two provided elements.
        /// </summary>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public StructOrderedElements(T? item1, T? item2)
            : this(item1, item2, (IComparer<T?>?)null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="StructOrderedElements{T}"/> using the two provided elements.
        /// </summary>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        /// <param name="comparer">The comparer to use for sorting.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public StructOrderedElements(T? item1, T? item2, IComparer<T?>? comparer)
            : this(item1, item2, comparer != null ? comparer.Compare : (Func<T?, T?, int>?)null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="OrderedElements{T}"/> using the two provided elements.
        /// </summary>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        /// <param name="comparer">The comparer func to use for sorting.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public StructOrderedElements(T? item1, T? item2, Func<T?, T?, int>? comparer)
        {
            comparer ??= Comparer<T?>.Default.Compare;
            Lesser = comparer(item1, item2) < 0 ? item1 : item2;
            Greater = comparer(item1, item2) >= 0 ? item1 : item2;
        }

        /// <summary>
        /// Deconstructs the object.
        /// </summary>
        /// <param name="lesser"></param>
        /// <param name="greater"></param>
        public void Deconstruct(out T? lesser, out T? greater)
        {
            lesser = Lesser;
            greater = Greater;
        }

        /// <summary>
        /// Gets the greater element.
        /// </summary>
        public override object? GetGreater() => Greater;

        /// <summary>
        /// Gets the lesser element.
        /// </summary>
        public override object? GetLesser() => Lesser;
    }
}
