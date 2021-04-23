using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class SllOperationSyntax : OperationSyntax
    {
        internal SllOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("sll", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Shift left logical - shift the contents of register {Instruction.RT} left by the distance";
                yield return $"# indicated in {Instruction.Shift} and store the result in register {Instruction.RD}.";
            }
            yield return $"sll\t{Instruction.RD}, {Instruction.RT}, {Instruction.Shift}";
        }
    }
}
