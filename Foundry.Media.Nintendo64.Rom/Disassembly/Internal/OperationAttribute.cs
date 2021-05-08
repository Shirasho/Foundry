using System;

namespace Foundry.Media.Nintendo64.Rom.Disassembly.Internal
{
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class OperationAttribute : Attribute
    {
        /// <summary>
        /// The operation instruction code.
        /// </summary>
        public string Instruction { get; }

        /// <summary>
        /// The extended name of the operation instruction.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The arguments.
        /// </summary>
        public string Arguments { get; }

        /// <summary>
        /// A short summary of the operation.
        /// </summary>
        public string Summary { get; }

        /// <summary>
        /// Any misnomers relating to this instruction code.
        /// </summary>
        public string? Misnomer { get; init; }

        /// <summary>
        /// The MIPS version this instruction was introduced in.
        /// </summary>
        public EMipsVersion IntroducedIn { get; init; }

        public OperationAttribute(string instruction, string name, string? argList, string summary)
        {
            Instruction = instruction;
            Name = name;
            Arguments = argList ?? string.Empty;
            Summary = summary;
        }
    }
}
