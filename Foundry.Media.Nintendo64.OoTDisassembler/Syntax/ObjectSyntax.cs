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
    /// Represents a MIPS object.
    /// </summary>
    public sealed class ObjectSyntax : ISyntax
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
        /// The address of this object.
        /// </summary>
        public uint Address { get; }

        /// <summary>
        /// The properties of this object.
        /// </summary>
        public IImmutableList<VariableSyntax> Properties { get; }

        /// <summary>
        /// The functions of this object.
        /// </summary>
        public IImmutableList<FunctionSyntax> Functions { get; }

        /// <summary>
        /// The objects/subclasses of this object.
        /// </summary>
        public IImmutableList<ObjectSyntax> SubObjects { get; }

        internal ObjectSyntax(string name, uint address, IEnumerable<VariableSyntax> properties, IEnumerable<FunctionSyntax> functions, IEnumerable<ObjectSyntax>? subObjects = null)
        {
            Name = name;
            Address = address;
            Properties = properties.ToImmutableArray();
            Functions = functions.ToImmutableArray();
            SubObjects = subObjects?.ToImmutableArray() ?? ImmutableArray<ObjectSyntax>.Empty;
        }

        public ValueTask WriteToAsync(StringBuilder builder, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public ValueTask WriteToAsync(TextWriter writer, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
