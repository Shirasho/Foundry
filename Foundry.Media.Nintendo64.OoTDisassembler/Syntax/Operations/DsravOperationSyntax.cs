using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class DsravOperationSyntax : OperationSyntax
    {
        internal DsravOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("dsrav", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# ";
            }
            yield return $"dsrav\t{Instruction.RD}, {Instruction.RT}, {Instruction.RS}";
        }
    }
}
