using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class MtloOperationSyntax : OperationSyntax
    {
        internal MtloOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("mtlo", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Stores the value in register {Instruction.RS} in the special register LO.";
            }
            yield return $"mtlo\t{Instruction.RS}";
        }
    }
}
