using System;
using System.Collections.Generic;

namespace Foundry.Media.Nintendo64.Rom
{
    /// <summary>
    /// ROM size information.
    /// </summary>
    public sealed class RomSize : IEquatable<RomSize>, IComparable<RomSize>, IEquatable<ContentSize>, IComparable<ContentSize>
    {
        private static readonly IReadOnlyList<ulong> ValidRomSizes = new ulong[]{
            // 32Mbit
            4194304,
            // 64Mbit
            8388608,
            // 96Mbit
            12582912,
            // 128Mbit
            16777216,
            // 256Mbit
            33554432,
            // 320Mbit
            41943040,
            // 512MBit
            67108864
        };

        /// <summary>
        /// A <see cref="RomSize"/> representing 0 bytes.
        /// </summary>
        public static RomSize Empty { get; } = new RomSize(0);

        /// <summary>
        /// The size of the ROM data.
        /// </summary>
        public ContentSize Size { get; }

        /// <summary>
        /// The number of overdump bytes.
        /// </summary>
        public ulong OverdumpSize { get; }

        /// <summary>
        /// Whether the ROM data has any overdump bytes.
        /// </summary>
        public bool HasOverdump => OverdumpSize != 0;

        public RomSize(ulong bytes)
            : this(new ContentSize(bytes))
        {

        }

        public RomSize(ContentSize size)
        {
            Size = size;
            OverdumpSize = 0;

            for (int i = 0; i < ValidRomSizes.Count; ++i)
            {
                ulong bytes = ValidRomSizes[i];
                if (Size.Bytes >= bytes || i == ValidRomSizes.Count - 1)
                {
                    OverdumpSize = Size.Bytes - bytes;
                    break;
                }
            }
        }

        public bool Equals(RomSize? other)
        {
            return other is not null && Size == other.Size;
        }

        public int CompareTo(RomSize? other)
        {
            if (other is null)
            {
                return 1;
            }

            return Size.CompareTo(other.Size);
        }

        public bool Equals(ContentSize other)
        {
            return Size.Equals(other);
        }

        public int CompareTo(ContentSize other)
        {
            return Size.CompareTo(other);
        }
    }
}
