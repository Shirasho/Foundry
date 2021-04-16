using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Foundry.Media.Nintendo64.Rom;

namespace Foundry.Media.Nintendo64.OotDecompiler
{
    internal static class RomDataExtensions
    {
        public static RomBuild GetRomBuild(this IRomData romData)
        {
            return RomBuild.Create(romData);
        }

        public static ReadOnlyMemory<byte> GetCodeData(this IRomData romData)
        {
            return romData.GetData().Slice(0x1000, 0x100000);
        }

        public static async ValueTask<ReadOnlyMemory<byte>> GetCodeDataAsync(this IRomData romData)
        {
            var data = await romData.GetDataAsync();
            return data.Slice(0x1000, 0x100000);
        }

        public static bool AssertValidEntryPoint(this IRomData romData)
        {
            bool isEntryAddressValid = romData.Metadata.EntryAddress >= 0x1000 && romData.Metadata.EntryAddress < 0x100000;
            Debug.Assert(isEntryAddressValid);

            return isEntryAddressValid;
        }
    }
}
