using System.Threading.Tasks;
using Foundry.Media.Nintendo64.Rom;

namespace Foundry.Media.Nintendo64.OotDecompiler
{
    internal static class RomDataExtensions
    {
        public static RomBuild GetRomBuild(this IRomData romData)
        {
            var data = romData.GetData();
            return RomBuild.Create(data.Span);
        }

        public static async ValueTask<RomBuild> GetRomBuildAsync(this IRomData romData)
        {
            var data = await romData.GetDataAsync();
            return RomBuild.Create(data.Span);
        }
    }
}
