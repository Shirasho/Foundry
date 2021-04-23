using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    /// <summary>
    /// Represents a MIPS function.
    /// </summary>
    public sealed class FunctionSyntax : ISyntax
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
        /// The address of this function.
        /// </summary>
        public uint Address { get; }

        /// <summary>
        /// A list of operations that make up this method.
        /// </summary>
        public IImmutableList<OperationSyntax> Operations { get; }

        /// <summary>
        /// A list of arguments for this method.
        /// </summary>
        public IImmutableList<ArgumentSyntax> Arguments { get; }

        internal FunctionSyntax(string name, uint address, IEnumerable<ArgumentSyntax> arguments, IEnumerable<OperationSyntax> operations)
        {
            Name = name;
            Address = address;
            Arguments = arguments.ToImmutableArray();
            Operations = operations.ToImmutableArray();
        }

        public ValueTask WriteToAsync(StringBuilder builder, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public ValueTask WriteToAsync(TextWriter writer, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
