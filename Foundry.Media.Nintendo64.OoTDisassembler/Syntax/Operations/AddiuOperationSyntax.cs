using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class AddiuOperationSyntax : OperationSyntax
    {
        internal AddiuOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("addiu", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Puts the sum of the value in register {Instruction.RS} and the immediate";
                yield return $"# value 0x{Instruction.Immediate:X} into register {Instruction.RT}.";
            }
            yield return $"addiu\t{Instruction.RT}, {Instruction.RS}, {Instruction.INN}";
        }
    }
}
