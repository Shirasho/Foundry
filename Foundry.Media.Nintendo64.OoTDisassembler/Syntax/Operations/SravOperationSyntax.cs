using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class SravOperationSyntax : OperationSyntax
    {
        internal SravOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("srav", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Shift right arithmetic variable - shift the contents of register {Instruction.RT} right by the";
                yield return $"# distance indicated in value stored in register {Instruction.RS} and store the result in";
                yield return $"# register {Instruction.RD}. Related to 2's-complement - the sign bit is preserved.";
            }
            yield return $"srav\t{Instruction.RD}, {Instruction.RT}, {Instruction.RS}";
        }
    }
}
