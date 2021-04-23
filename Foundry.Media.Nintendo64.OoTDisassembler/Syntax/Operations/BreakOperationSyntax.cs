using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class BreakOperationSyntax : OperationSyntax
    {
        internal BreakOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("break", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# ";
            }
            yield return $"break\t{(Instruction.RawInstruction & (0xFFFFF << 6)) >> 16:X}";
        }
    }
}
