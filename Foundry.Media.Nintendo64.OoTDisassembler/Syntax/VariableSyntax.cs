using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    /// <summary>
    /// Represents a MIPS variable.
    /// </summary>
    public sealed class VariableSyntax : ISyntax
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
        /// The address of this variable.
        /// </summary>
        public uint Address { get; }

        internal VariableSyntax(string name, uint address)
        {
            Name = name;
            Address = address;
        }

        public ValueTask WriteToAsync(StringBuilder builder, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public ValueTask WriteToAsync(TextWriter writer, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
    }
}
