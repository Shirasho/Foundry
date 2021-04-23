using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class DsllvOperationSyntax : OperationSyntax
    {
        internal DsllvOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("dsllv", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# ";
            }
            yield return $"dsllv\t{Instruction.RD}, {Instruction.RT}, {Instruction.RS}";
        }
    }
}
