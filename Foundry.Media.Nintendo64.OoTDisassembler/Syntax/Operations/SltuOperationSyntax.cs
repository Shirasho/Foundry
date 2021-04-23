using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class SltuOperationSyntax : OperationSyntax
    {
        internal SltuOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("sltu", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# SET LESS THAN UNSIGNED";
            }
            yield return $"sltu\t{Instruction.RD}, {Instruction.RS}, {Instruction.RT}";
        }
    }
}
