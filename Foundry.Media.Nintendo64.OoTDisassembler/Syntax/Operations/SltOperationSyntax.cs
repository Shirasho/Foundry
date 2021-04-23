using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class SltOperationSyntax : OperationSyntax
    {
        internal SltOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("slt", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# SET LESS THAN";
            }
            yield return $"slt\t{Instruction.RD}, {Instruction.RS}, {Instruction.RT}";
        }
    }
}
