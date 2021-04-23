using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class JrOperationSyntax : OperationSyntax
    {
        internal JrOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("jr", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Unconditionally jump (goto) to the instruction whose address is in register {Instruction.RS}.";
            }
            yield return $"jr\t{Instruction.RS}";
        }
    }
}
