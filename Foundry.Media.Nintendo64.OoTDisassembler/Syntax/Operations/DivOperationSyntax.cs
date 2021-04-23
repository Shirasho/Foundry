using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class DivOperationSyntax : OperationSyntax
    {
        internal DivOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("div", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Divides the value in register {Instruction.RS} by the value in register {Instruction.RT}.";
                yield return $"# The quotient is in register LO and the remainder is in register HI. These can be accessed";
                yield return $"# by calling {FunctionCode.mflo.Name} and {FunctionCode.mfhi.Name} respectively.";
            }
            yield return $"div\t{Instruction.RS}, {Instruction.RT}";
        }
    }
}
