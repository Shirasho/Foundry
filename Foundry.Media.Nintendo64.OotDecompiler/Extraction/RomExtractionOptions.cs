using System;
using System.IO;

namespace Foundry.Media.Nintendo64.OotDecompiler.Extraction
{
    /// <summary>
    /// Rom extraction options.
    /// </summary>
    internal class RomExtractionOptions
    {
        /// <summary>
        /// The directory to extract the output to.
        /// </summary>
        public DirectoryInfo OutputDirectory { get; set; } = new DirectoryInfo(AppContext.BaseDirectory);
    }
}
