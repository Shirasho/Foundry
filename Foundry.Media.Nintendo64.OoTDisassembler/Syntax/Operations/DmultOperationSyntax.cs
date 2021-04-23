using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class DmultOperationSyntax : OperationSyntax
    {
        internal DmultOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("dmult", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# ";
            }
            yield return $"dmult\t{Instruction.RS}, {Instruction.RT}";
        }
    }
}
