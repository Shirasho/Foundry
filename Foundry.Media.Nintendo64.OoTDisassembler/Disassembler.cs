using System.Threading;
using System.Threading.Tasks;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Media.Nintendo64.Disassembly.OoT
{
    /// <summary>
    /// The entry/extension point for MIPS disassembly.
    /// </summary>
    /// <remarks>
    /// This class can have extension methods to generate difference source
    /// code by creating different <see cref="ISourceBuilder"/> implementations
    /// and passing them to <see cref="DisassembleAsync(DisassemblerOptions, ISourceBuilder, CancellationToken)"/>.
    /// </remarks>
    public sealed class Disassembler
    {
        /// <summary>
        /// Disassembles the ROM.
        /// </summary>
        /// <param name="options">Options for the disassembler.</param>
        /// <param name="sourceBuilder">The builder that will convert the MIPS syntax tree into source code.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task DisassembleAsync(DisassemblerOptions options, ISourceBuilder sourceBuilder, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(options, nameof(options));
            Guard.IsNotNull(sourceBuilder, nameof(sourceBuilder));

            await using var disassembler = new MipsDisassembler(options, sourceBuilder);
            await disassembler.DisassembleAsync(cancellationToken);
        }
    }
}
