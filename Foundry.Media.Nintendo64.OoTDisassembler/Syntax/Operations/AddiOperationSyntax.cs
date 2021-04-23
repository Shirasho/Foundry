using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class AddiOperationSyntax : OperationSyntax
    {
        internal AddiOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("addi", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Puts the sum of the value in register {Instruction.RS} and the immediate";
                yield return $"# value 0x{Instruction.Immediate:X} into register {Instruction.RT}.";
            }
            yield return $"addi\t{Instruction.RT}, {Instruction.RS}, {Instruction.Immediate}";
        }
    }
}
