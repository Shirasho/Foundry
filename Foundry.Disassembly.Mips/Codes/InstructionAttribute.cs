using System;

namespace Foundry.Disassembly.Mips.Codes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class InstructionAttribute : Attribute
    {
        /// <summary>
        /// The instruction code.
        /// </summary>
        public string Instruction { get; }

        internal InstructionAttribute(string instruction)
        {
            Instruction = instruction;
        }
    }
}
