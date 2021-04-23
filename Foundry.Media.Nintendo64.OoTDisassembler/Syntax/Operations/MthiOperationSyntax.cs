using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class MthiOperationSyntax : OperationSyntax
    {
        internal MthiOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("mthi", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Stores the value in register {Instruction.RS} in the special register HI.";
            }
            yield return $"mthi\t{Instruction.RS}";
        }
    }
}
