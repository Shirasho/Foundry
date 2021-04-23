using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class Dsra32OperationSyntax : OperationSyntax
    {
        internal Dsra32OperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("dsra32", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# ";
            }
            yield return $"dsra32\t{Instruction.RD}, {Instruction.RT}, {Instruction.Shift}";
        }
    }
}
