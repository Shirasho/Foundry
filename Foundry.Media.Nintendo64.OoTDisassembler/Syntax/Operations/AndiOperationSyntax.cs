using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class AndiOperationSyntax : OperationSyntax
    {
        internal AndiOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("andi", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Bitwise ANDs the value in register {Instruction.RS} and the immediate";
                yield return $"# value 0x{Instruction.Immediate} and stores the result in register {Instruction.RD}.";
            }
            yield return $"andi\t{Instruction.RD}, {Instruction.RS}, {Instruction.Immediate}";
        }
    }
}
