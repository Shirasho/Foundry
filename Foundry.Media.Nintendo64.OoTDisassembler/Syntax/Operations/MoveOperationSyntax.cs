using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class MoveOperationSyntax : OperationSyntax
    {
        internal MoveOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("move", instruction, owner)
        {

        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Move the contents of register {Instruction.RD} into register {Instruction.RS}";
            }
            yield return $"move\t{Instruction.RD}, {Instruction.RS}";
        }
    }
}
