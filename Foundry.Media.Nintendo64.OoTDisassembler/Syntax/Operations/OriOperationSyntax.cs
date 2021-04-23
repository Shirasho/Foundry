using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class OriOperationSyntax : OperationSyntax
    {
        internal OriOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("ori", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Bitwise ORs the value in register {Instruction.RS} and the immediate";
                yield return $"# value 0x{Instruction.Immediate} and stores the result in register {Instruction.RD}.";
            }
            yield return $"ori\t{Instruction.RD}, {Instruction.RS}, {Instruction.Immediate}";
        }
    }
}
