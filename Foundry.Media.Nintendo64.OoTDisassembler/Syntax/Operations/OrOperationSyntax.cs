using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class OrOperationSyntax : OperationSyntax
    {
        internal OrOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("or", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Bitwise ORs the values in registers {Instruction.RS} and {Instruction.RT}";
                yield return $"# and stores the result in register {Instruction.RD}.";
            }
            yield return $"or\t{Instruction.RD}, {Instruction.RS}, {Instruction.RT}";
        }
    }
}
