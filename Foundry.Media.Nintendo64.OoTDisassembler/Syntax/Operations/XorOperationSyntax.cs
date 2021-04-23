using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class XorOperationSyntax : OperationSyntax
    {
        internal XorOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("xor", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Bitwise XORs the values in registers {Instruction.RS} and {Instruction.RT}";
                yield return $"# and stores the result in register {Instruction.RD}.";
            }
            yield return $"xor\t{Instruction.RD}, {Instruction.RS}, {Instruction.RT}";
        }
    }
}
