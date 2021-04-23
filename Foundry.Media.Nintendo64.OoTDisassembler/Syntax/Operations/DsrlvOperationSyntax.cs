using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class DsrlvOperationSyntax : OperationSyntax
    {
        internal DsrlvOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("dsrlv", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# ";
            }
            yield return $"dsrlv\t{Instruction.RD}, {Instruction.RT}, {Instruction.RS}";
        }
    }
}
