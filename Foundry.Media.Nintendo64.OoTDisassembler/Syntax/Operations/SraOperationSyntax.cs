using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class SraOperationSyntax : OperationSyntax
    {
        internal SraOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("sra", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Shift right arithmetic - shift the contents of register {Instruction.RT} right by the distance";
                yield return $"# indicated in {Instruction.Shift} and store the result in register {Instruction.RD}.";
                yield return  "# Related to 2's-complement - the sign bit is preserved.";
            }
            yield return $"sra\t{Instruction.RD}, {Instruction.RT}, {Instruction.Shift}";
        }
    }
}
