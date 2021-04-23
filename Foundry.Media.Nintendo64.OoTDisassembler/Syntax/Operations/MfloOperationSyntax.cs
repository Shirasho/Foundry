using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class MfloOperationSyntax : OperationSyntax
    {
        internal MfloOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("mflo", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Stores the value in the special register LO in register {Instruction.RD}.";
            }
            yield return $"mflo\t{Instruction.RD}";
        }
    }
}
