using System;

namespace Foundry
{
    /// <summary>
    /// Represents the size of certain content.
    /// </summary>
    public readonly struct ContentSize : IEquatable<ContentSize>, IComparable<ContentSize>
    {
        /// <summary>
        /// Content with no size.
        /// </summary>
        public static ContentSize None => new ContentSize();

        /// <summary>
        /// The size of this content in terabytes.
        /// </summary>
        public readonly double Terabytes => Gigabytes / 1000;

        /// <summary>
        /// The size of this content in tebibits.
        /// </summary>
        public readonly double Tebibits => Gibibits / 1024;

        /// <summary>
        /// The size of this content in gigabytes.
        /// </summary>
        public readonly double Gigabytes => Megabytes / 1000;

        /// <summary>
        /// The size of this content in gibibits.
        /// </summary>
        public readonly double Gibibits => Mebibits / 1024;

        /// <summary>
        /// The size of this content in megabytes.
        /// </summary>
        public readonly double Megabytes => Kilobytes / 1000;

        /// <summary>
        /// The size of this content in mebibits.
        /// </summary>
        public readonly double Mebibits => Kibibits / 1024;

        /// <summary>
        /// The size of this content in kilobytes.
        /// </summary>
        public readonly double Kilobytes => Bytes / 1000;

        /// <summary>
        /// The size of this content in kibibits.
        /// </summary>
        public readonly double Kibibits => Bytes / 1024;

        /// <summary>
        /// The size of this content in bytes.
        /// </summary>
        public readonly long Bytes { get; }

        public ContentSize(long bytes)
        {
            Bytes = bytes;
        }

        public static ContentSize FromKibibits(double kibibits)
        {
            return new ContentSize((long)(kibibits * 1024));
        }

        public static ContentSize FromMebibits(double mebibits)
        {
            return new ContentSize((long)(mebibits * 1024 * 1024));
        }

        public static ContentSize FromGibibits(double gibibits)
        {
            return new ContentSize((long)(gibibits * 1024 * 1024 * 1024));
        }

        public readonly int CompareTo(ContentSize other)
        {
            return Bytes.CompareTo(other.Bytes);
        }

        public readonly bool Equals(ContentSize other)
        {
            return Bytes == other.Bytes;
        }

        public override readonly string ToString()
        {
            return $"{Megabytes:F2}MB";
        }

        public override int GetHashCode()
        {
            return Bytes.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return obj is ContentSize cs && cs.Equals(this);
        }

        public static bool operator ==(ContentSize left, ContentSize right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ContentSize left, ContentSize right)
        {
            return !(left == right);
        }

        public static bool operator <(ContentSize left, ContentSize right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(ContentSize left, ContentSize right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(ContentSize left, ContentSize right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(ContentSize left, ContentSize right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}
