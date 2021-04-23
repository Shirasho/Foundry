using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    /// <summary>
    /// Represents a MIPS syntax element.
    /// </summary>
    public interface ISyntax
    {
        /// <summary>
        /// The name of this syntax element.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The owner of this syntax element.
        /// </summary>
        ISyntax? Owner { get; }

        /// <summary>
        /// The instruction that generated this syntax.
        /// </summary>
        Instruction Instruction { get; }

        /// <summary>
        /// Writes this element to the provided <see cref="StringBuilder"/> instance.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> instance to write to.</param>
        /// <param name="includeComments">Whether to include comments above the MIPS instructions describing the operation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        ValueTask WriteToAsync(StringBuilder builder, bool includeComments = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes this element to the provided <see cref="TextWriter"/> instance.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        ValueTask WriteToAsync(TextWriter writer, bool includeComments = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes this element to the provided <see cref="ISourceBuilder"/> instance.
        /// </summary>
        /// <param name="builder">The <see cref="ISourceBuilder"/> instance to write to.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        ValueTask WriteToAsync(ISourceBuilder builder, CancellationToken cancellationToken = default)
        {
            return builder.WriteAsync(this, cancellationToken);
        }
    }
}
