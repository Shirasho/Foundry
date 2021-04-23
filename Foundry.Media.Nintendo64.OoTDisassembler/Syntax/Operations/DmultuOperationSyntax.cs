using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class DmultuOperationSyntax : OperationSyntax
    {
        internal DmultuOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("dmultu", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# ";
            }
            yield return $"dmultu\t{Instruction.RS}, {Instruction.RT}";
        }
    }
}
