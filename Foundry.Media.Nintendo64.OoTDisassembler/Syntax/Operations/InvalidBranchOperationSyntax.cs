using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class InvalidBranchOperationSyntax : OperationSyntax
    {
        private readonly Instruction Addr;
        private readonly int InstructionOffset;

        internal InvalidBranchOperationSyntax(ISyntax? owner, in Instruction instruction, in Instruction addr, int instructionOffset)
            // branch1reg_erro
            : base("branch_error", instruction, owner)
        {
            Addr = addr;
            InstructionOffset = instructionOffset;
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            // branch1reg_erro
            yield return $"/* branch_error: {Instruction.RT} */";
            yield return $"/* 0x{InstructionOffset:X} {Addr} */ .word\t{Instruction}";
        }
    }
}
