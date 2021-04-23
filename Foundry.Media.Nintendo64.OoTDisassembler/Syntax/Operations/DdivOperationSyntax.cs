using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class DdivOperationSyntax : OperationSyntax
    {
        internal DdivOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("ddiv", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# ";
            }
            yield return $"ddiv\t{Instruction.RS}, {Instruction.RT}";
        }
    }
}
