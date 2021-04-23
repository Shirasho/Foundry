using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class JalrOperationSyntax : OperationSyntax
    {
        internal JalrOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("jalr", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# ";
            }
            yield return $"jalr\t{Instruction.RS}";
        }
    }
}
