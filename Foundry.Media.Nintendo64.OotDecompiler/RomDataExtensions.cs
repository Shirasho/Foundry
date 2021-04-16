using Foundry.Media.Nintendo64.Rom;

namespace Foundry.Media.Nintendo64.OotDecompiler
{
    internal static class RomDataExtensions
    {
        public static RomBuild GetRomBuild(this IRomData romData)
        {
            return RomBuild.Create(romData);
        }
    }
}
