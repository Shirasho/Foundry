using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class XoriOperationSyntax : OperationSyntax
    {
        internal XoriOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("xori", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Bitwise XORs the value in register {Instruction.RS} and the immediate";
                yield return $"# value 0x{Instruction.Immediate} and stores the result in register {Instruction.RD}.";
            }
            yield return $"xori\t{Instruction.RD}, {Instruction.RS}, {Instruction.Immediate}";
        }
    }
}
