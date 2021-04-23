using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class LuiOperationSyntax : OperationSyntax
    {
        internal LuiOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("lui", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Loads the lower halfword of the immediate value 0x{Instruction.Immediate:X}";
                yield return $"# into the upper halfword of register {Instruction.RT}. The lower bits of the register";
                yield return $"# are set to 0.";
            }
            yield return $"lui\t{Instruction.RT}, {Instruction.Immediate}";
        }
    }
}
