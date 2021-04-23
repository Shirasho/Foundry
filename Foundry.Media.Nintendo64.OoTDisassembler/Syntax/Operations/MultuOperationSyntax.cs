using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class MultuOperationSyntax : OperationSyntax
    {
        internal MultuOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("multu", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Multiplies the values in registers {Instruction.RS} and {Instruction.RT}.";
                yield return $"# The low-order word of the product is stored in special register LO and";
                yield return $"# the high-order word of the product is stored in special register HI.";
                yield return $"# These can be accessed by calling {FunctionCode.mflo.Name} and {FunctionCode.mfhi.Name} respectively.";
            }
            yield return $"multu\t{Instruction.RS}, {Instruction.RT}";
        }
    }
}
