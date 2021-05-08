using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Foundry.Media.Nintendo64.Rom.Disassembly
{
    /// <summary>
    /// The result of a disassembly.
    /// </summary>
    public class DisassemblyResult
    {
        /// <summary>
        /// Whether the disassembly occurred successfully.
        /// </summary>
        [MemberNotNullWhen(false, nameof(Exception))]
        public bool IsSuccess => Exception is null;

        /// <summary>
        /// The syntax elements of the parse. Note that this may contain
        /// error elements. Ensure you call <see cref="DisassemblySyntax.IsErrorSyntax"/>
        /// before use of each element.
        /// </summary>
        public IReadOnlyCollection<Operation> Syntax { get; }

        /// <summary>
        /// The exception that occurred.
        /// </summary>
        public Exception? Exception { get; }

        internal DisassemblyResult(IReadOnlyCollection<Operation> syntax)
        {
            Syntax = syntax;
        }

        internal DisassemblyResult(Exception e)
        {
            Syntax = Array.Empty<Operation>();
            Exception = e;
        }
    }
}
