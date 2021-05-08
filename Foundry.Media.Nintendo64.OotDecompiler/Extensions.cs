using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using Foundry.Media.Nintendo64.OotDecompiler.Tools;
using Foundry.Media.Nintendo64.Rom;

namespace Foundry.Media.Nintendo64.OotDecompiler
{
    internal static class Extensions
    {
        /// <summary>
        /// Returns a new <see cref="RomHeader"/> instance with the header entry address adjusted
        /// based on the CIC.
        /// </summary>
        /// <remarks>
        /// This method may return the original <see cref="RomHeader"/>.
        /// </remarks>
        /// <param name="header">The old header data.</param>
        /// <param name="data">The ROM data.</param>
        public static RomHeader WithCalculatedEntryOffset(this RomHeader header, in ReadOnlySpan<byte> data)
        {
            int cic = Crc.GetCIC(data);
            if (cic == 6103)
            {
                return header.WithEntryOffset(-0x100000);
            }
            else if (cic == 6106)
            {
                return header.WithEntryOffset(-0x200000);
            }

            return header;
        }

        /// <summary>
        /// Returns the OoT build info.
        /// </summary>
        /// <param name="romData">The ROM data.</param>
        public static RomBuild GetRomBuild(this IRomData romData)
        {
            return RomBuild.Create(romData);
        }

        /// <summary>
        /// Returns the ROM data decompressed.
        /// </summary>
        /// <param name="romData">The ROM data.</param>
        public static IMemoryOwner<byte> Decompress(this IRomData romData)
        {
            var data = romData.GetData();
            return Decompress(data);
        }

        /// <summary>
        /// Returns the ROM data decompressed.
        /// </summary>
        /// <param name="romData">The ROM data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static async ValueTask<IMemoryOwner<byte>> DecompressAsync(this IRomData romData, CancellationToken cancellationToken = default)
        {
            // End - start gives us the number of addresses, and each address is 4 bytes.
            var data = await romData.GetDataAsync(cancellationToken);
            return Decompress(data);
        }

        private static IMemoryOwner<byte> Decompress(in ReadOnlyMemory<byte> data)
        {
            return new Decompressor().Decompress(data, new uint[] { 0x00000000, 0x60100000 });
        }

        /// <summary>
        /// Returns the code region from a memory region that has been decompressed with <see cref="Decompress(IRomData)"/>
        /// or <see cref="DecompressAsync(IRomData, CancellationToken)"/>.
        /// </summary>
        /// <param name="data">The decompressed ROM data.</param>
        /// <param name="endAddress">The end address of the code region.</param>
        public static ReadOnlyMemory<byte> GetCodeData(this IMemoryOwner<byte> data, uint endAddress)
            => GetCodeData(data, RomData.CodeStartAddress, endAddress);

        /// <summary>
        /// Returns the code region from a memory region that has been decompressed with <see cref="Decompress(IRomData)"/>
        /// or <see cref="DecompressAsync(IRomData, CancellationToken)"/>.
        /// </summary>
        /// <param name="data">The decompressed ROM data.</param>
        /// <param name="startAddress">The start address of the code region.</param>
        /// <param name="endAddress">The end address of the code region.</param>
        public static ReadOnlyMemory<byte> GetCodeData(this IMemoryOwner<byte> data, uint startAddress, uint endAddress)
        {
            return data.Memory.Slice((int)startAddress, (int)(endAddress - startAddress));
        }
    }
}
