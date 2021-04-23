using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class Dsll32OperationSyntax : OperationSyntax
    {
        internal Dsll32OperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("dsll32", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# ";
            }
            yield return $"dsll32\t{Instruction.RD}, {Instruction.RT}, {Instruction.Shift}";
        }
    }
}
