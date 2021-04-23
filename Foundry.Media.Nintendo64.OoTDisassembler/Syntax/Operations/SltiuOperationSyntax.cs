using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class SltiuOperationSyntax : OperationSyntax
    {
        internal SltiuOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("sltiu", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# ";
            }
            yield return $"sltiu\t{Instruction.RT}, {Instruction.RS}, {Instruction.Immediate}";
        }
    }
}
