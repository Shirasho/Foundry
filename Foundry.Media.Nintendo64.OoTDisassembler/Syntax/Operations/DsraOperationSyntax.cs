using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class DsraOperationSyntax : OperationSyntax
    {
        internal DsraOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("dsra", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# ";
            }
            yield return $"dsra\t{Instruction.RD}, {Instruction.RT}, {Instruction.Shift}";
        }
    }
}
