using System.Threading;
using System.Threading.Tasks;
using Foundry.Media.Nintendo64.Disassembly.OoT.Syntax;

namespace Foundry.Media.Nintendo64.Disassembly.OoT
{
    /// <summary>
    /// Represents a service that can turn MIPS into source code.
    /// </summary>
    public interface ISourceBuilder
    {
        /// <summary>
        /// Whether to split objects/classes into separate files.
        /// </summary>
        bool SplitFiles { get; }

        /// <summary>
        /// Converts the provided <see cref="ISyntax"/> element into source code and writes
        /// the output to a destination.
        /// </summary>
        /// <param name="syntax">The element to write.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        ValueTask WriteAsync(ISyntax syntax, CancellationToken cancellationToken = default);
    }
}
