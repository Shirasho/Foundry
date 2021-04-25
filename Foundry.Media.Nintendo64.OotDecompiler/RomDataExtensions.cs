using System;
using System.Threading.Tasks;
using Foundry.Media.Nintendo64.Rom;

namespace Foundry.Media.Nintendo64.OotDecompiler
{
    public static class RomDataExtensions
    {
        private const int CodeAddressStart = 0x1000;

        // We don't know what this is, but we can figure this out
        // when we start disassembling correctly and getting a bunch of opcode errors.
        // For reference, we know that Mario ends at 0x0B6A40. We'll go a bit higher then
        // work our way down.
        private const int CodeEndAddress = 0x0C3500;

        public static RomBuild GetRomBuild(this IRomData romData)
        {
            return RomBuild.Create(romData);
        }

        public static ReadOnlyMemory<byte> GetHeaderData(this IRomData romData)
        {
            return romData.GetData().Slice(0, RomHeader.Length);
        }

        public static async ValueTask<ReadOnlyMemory<byte>> GetHeaderDataAsync(this IRomData romData)
        {
            var data = await romData.GetDataAsync();
            return data.Slice(0, RomHeader.Length);
        }

        public static ReadOnlyMemory<byte> GetCodeData(this IRomData romData)
        {
            // End - start gives us the number of addresses, and each address is 4 bytes.
            return romData.GetData().Slice(CodeAddressStart, (CodeEndAddress - CodeAddressStart) * 4);
        }

        public static async ValueTask<ReadOnlyMemory<byte>> GetCodeDataAsync(this IRomData romData)
        {
            var data = await romData.GetDataAsync();
            // End - start gives us the number of addresses, and each address is 4 bytes.
            return data.Slice(CodeAddressStart, (CodeEndAddress - CodeAddressStart) * 4);
        }
    }
}
