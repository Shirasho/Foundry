using System;

namespace Foundry.Media.Nintendo64.RomData
{
    /// <summary>
    /// ROM size information.
    /// </summary>
    public readonly struct RomSize
    {
        /// <summary>
        /// The size of the ROM data.
        /// </summary>
        public readonly ContentSize Size { get; }

        /// <summary>
        /// The number of overdump bytes.
        /// </summary>
        public readonly long OverdumpSize { get; }

        /// <summary>
        /// Whether the ROM data has any overdump bytes.
        /// </summary>
        public readonly bool HasOverdump => OverdumpSize != 0;

        public RomSize(long bytes)
            : this(new ContentSize(bytes))
        {

        }

        public RomSize(ContentSize size)
        {
            Size = size;
            OverdumpSize = 0;

            Span<long> validRomSizes = stackalloc long[] {
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

            for (int i = 0; i < (uint)validRomSizes.Length; ++i)
            {
                long bytes = validRomSizes[i];
                if (Size.Bytes >= bytes || i == validRomSizes.Length - 1)
                {
                    OverdumpSize = Size.Bytes - bytes;
                    break;
                }
            }
        }
    }
}
