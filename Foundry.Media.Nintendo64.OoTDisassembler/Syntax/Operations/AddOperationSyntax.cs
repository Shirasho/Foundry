using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class AddOperationSyntax : OperationSyntax
    {
        internal AddOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("add", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Puts the sum of the values in register {Instruction.RS} and {Instruction.RT}";
                yield return $"# into register {Instruction.RD}.";
            }
            yield return $"add\t{Instruction.RD}, {Instruction.RS}, {Instruction.RT}";
        }
    }
}
