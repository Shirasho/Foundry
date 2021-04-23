using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class MulOperationSyntax : OperationSyntax
    {
        internal MulOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("mul", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Multiplies the values in registers {Instruction.RS} and {Instruction.RT}";
                yield return $"# and stores the low-order 32 bits of the result in register {Instruction.RD}.";
            }
            yield return $"mul\t{Instruction.RD}, {Instruction.RS}, {Instruction.RT}";
        }
    }
}
