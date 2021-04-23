using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class NegOperationSyntax : OperationSyntax
    {
        internal NegOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("neg", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Puts the negated value in register {Instruction.RT} into register {Instruction.RD}.";
            }
            yield return $"neg\t{Instruction.RD}, {Instruction.RT}";
        }
    }
}
