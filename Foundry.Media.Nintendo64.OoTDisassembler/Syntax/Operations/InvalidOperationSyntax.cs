using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class InvalidOperationSyntax : OperationSyntax
    {
        private readonly Instruction Addr;
        private readonly int InstructionOffset;

        internal InvalidOperationSyntax(ISyntax? owner, in Instruction instruction, in Instruction addr, int instructionOffset)
            : base("error", instruction, owner)
        {
            Addr = addr;
            InstructionOffset = instructionOffset;
        }
        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            yield return $"/* error: {Instruction.OperationCode} */";
            yield return $"/* 0x{InstructionOffset:X} {Addr} */ .word\t{Instruction}";
        }
    }
}
