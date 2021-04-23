using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class MfhiOperationSyntax : OperationSyntax
    {
        internal MfhiOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("mfhi", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Stores the value in the special register HI in register {Instruction.RD}.";
            }
            yield return $"mfhi\t{Instruction.RD}";
        }
    }
}
