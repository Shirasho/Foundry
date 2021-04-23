using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class InvalidFunctionOperationSyntax : OperationSyntax
    {
        private readonly Instruction Addr;
        private readonly int InstructionOffset;

        internal InvalidFunctionOperationSyntax(ISyntax? owner, in Instruction instruction, in Instruction addr, int instructionOffset)
            : base("func_error", instruction, owner)
        {
            Addr = addr;
            InstructionOffset = instructionOffset;
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            yield return $"/* func_error: {Instruction.FunctionCode} */";
            yield return $"/* 0x{InstructionOffset:X} {Addr} */ .word\t{Instruction}";
        }
    }
}
