using System.Threading;
using System.Threading.Tasks;

namespace Foundry.Media.Nintendo64.OotDecompiler.Extraction
{
    /// <summary>
    /// Defines a class that extracts certain data elements.
    /// </summary>
    internal interface IRomExtractor
    {
        /// <summary>
        /// Performs extraction on the ROM.
        /// </summary>
        /// <param name="options">The ROM extraction options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><see langword="true"/> if the extraction succeeded, <see langword="false"/> otherwise.</returns>
        Task<bool> ExtractAsync(RomExtractionOptions options, CancellationToken cancellationToken = default);
    }
}
