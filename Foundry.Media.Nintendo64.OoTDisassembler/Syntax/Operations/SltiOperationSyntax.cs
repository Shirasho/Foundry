using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class SltiOperationSyntax : OperationSyntax
    {
        internal SltiOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("slti", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# ";
            }
            yield return $"slti\t{Instruction.RT}, {Instruction.RS}, {Instruction.Immediate}";
        }
    }
}
