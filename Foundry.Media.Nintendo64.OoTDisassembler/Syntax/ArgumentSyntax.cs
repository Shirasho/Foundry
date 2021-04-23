using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    /// <summary>
    /// Represents an argument to a <see cref="FunctionSyntax"/>.
    /// </summary>
    public sealed class ArgumentSyntax : ISyntax
    {
        /// <summary>
        /// The name of this syntax element.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The owner of this syntax element.
        /// </summary>
        public ISyntax? Owner { get; }

        internal ArgumentSyntax(string name)
        {
            Name = name;
        }

        public ValueTask WriteToAsync(StringBuilder builder, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public ValueTask WriteToAsync(TextWriter writer, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
