using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Foundry.Media.Nintendo64.Rom;

namespace Foundry.Media.Nintendo64.Disassembly.OoT
{
    public static class RomDataExtensions
    {
        private const int CodeAddressStart = 0x1000;
        private const int CodeLength = 0x100000;

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
            return romData.GetData().Slice(CodeAddressStart, CodeLength);
        }

        public static async ValueTask<ReadOnlyMemory<byte>> GetCodeDataAsync(this IRomData romData)
        {
            var data = await romData.GetDataAsync();
            return data.Slice(CodeAddressStart, CodeLength);
        }

        internal static bool AssertValidEntryPoint(this IRomData romData)
        {
            bool isEntryAddressValid = romData.Header.EntryAddress >= 0x1000 && romData.Header.EntryAddress < 0x100000;
            Debug.Assert(isEntryAddressValid);

            return isEntryAddressValid;
        }
    }
}
