using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class SubuOperationSyntax : OperationSyntax
    {
        internal SubuOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("subu", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Puts the difference of the values in register {Instruction.RS} and {Instruction.RT}";
                yield return $"# into register {Instruction.RD}.";
            }
            yield return $"subu\t{Instruction.RD}, {Instruction.RS}, {Instruction.RT}";
        }
    }
}
