using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class DsllOperationSyntax : OperationSyntax
    {
        internal DsllOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("dsll", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# ";
            }
            yield return $"dsll\t{Instruction.RD}, {Instruction.RT}, {Instruction.Shift}";
        }
    }
}
