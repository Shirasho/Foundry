using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class NopOperationSyntax : OperationSyntax
    {
        internal NopOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("nop", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            yield return "nop";
        }
    }
}
