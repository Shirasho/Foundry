using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    /// <summary>
    /// Represents a MIPS operation/instruction.
    /// </summary>
    public abstract class OperationSyntax : ISyntax
    {
        /// <summary>
        /// The name of this syntax element.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The owner of this syntax element.
        /// </summary>
        public ISyntax? Owner { get; }

        /// <summary>
        /// The instruction that generated this syntax.
        /// </summary>
        public Instruction Instruction { get; }

        private protected OperationSyntax(string name, in Instruction instruction, ISyntax? owner)
        {
            Name = name;
            Instruction = instruction;
            Owner = owner;
        }

        /// <summary>
        /// Writes this element to the provided <see cref="StringBuilder" /> instance.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> instance to write to.</param>
        /// <param name="includeComments">Whether to include comments above the MIPS instructions describing the operation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public ValueTask WriteToAsync(StringBuilder builder, bool includeComments = true, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Guard.IsNotNull(builder, nameof(builder));

            foreach (string line in GetMIPSLines(includeComments))
            {
                cancellationToken.ThrowIfCancellationRequested();
                builder.AppendLine(line);
            }

            return default;
        }

        /// <summary>
        /// Writes this element to the provided <see cref="TextWriter" /> instance.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter" /> instance to write to.</param>
        /// <param name="includeComments">Whether to include comments above the MIPS instructions describing the operation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async ValueTask WriteToAsync(TextWriter writer, bool includeComments = true, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Guard.IsNotNull(writer, nameof(writer));

            foreach (string line in GetMIPSLines(includeComments))
            {
                await writer.WriteLineAsync(line, cancellationToken);
            }
        }

        /// <summary>
        /// Returns the operation as MIPS instructions.
        /// </summary>
        /// <param name="includeComments">Whether to include comments above the MIPS instructions describing the operation.</param>
        protected abstract IEnumerable<string> GetMIPSLines(bool includeComments);
    }
}
