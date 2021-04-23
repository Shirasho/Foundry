using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class AndOperationSyntax : OperationSyntax
    {
        internal AndOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("and", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Bitwise ANDs the values in registers {Instruction.RS} and {Instruction.RT}";
                yield return $"# and stores the result in register {Instruction.RD}.";
            }
            yield return $"and\t{Instruction.RD}, {Instruction.RS}, {Instruction.RT}";
        }
    }
}
