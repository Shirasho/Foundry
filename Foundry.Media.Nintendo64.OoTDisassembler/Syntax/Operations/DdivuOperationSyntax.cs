using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class DdivuOperationSyntax : OperationSyntax
    {
        internal DdivuOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("ddivu", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# ";
            }
            yield return $"ddivu\t{Instruction.RS}, {Instruction.RT}";
        }
    }
}
