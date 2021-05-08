using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Foundry.Media.Nintendo64.Rom.Disassembly
{
    /// <summary>
    /// Represents a MIPS operation.
    /// </summary>
    public sealed class Operation : OperationMetadata
    {
        public delegate string ToMIPSStringDelegate(string name, in Instruction instruction);

        /// <summary>
        /// Whether this operation is valid with no errors.
        /// </summary>
        [MemberNotNullWhen(false, nameof(Exception))]
        public bool IsValid => Code.IsDefined() && Code != EOperationCode.Invalid && Exception is null;

        /// <summary>
        /// The instruction for this operation.
        /// </summary>
        public Instruction Instruction { get; }

        /// <summary>
        /// The data file this instruction belongs to.
        /// </summary>
        public DataFile File { get; }

        /// <summary>
        /// Details about an error with this operation.
        /// </summary>
        public Exception? Exception { get; }

        internal readonly ToMIPSStringDelegate MIPSStringDelegate;

        internal Operation(EOperationCode code, in Instruction instruction, DataFile file, ToMIPSStringDelegate toMIPSString)
            : base(code)
        {
            Debug.Assert(code.IsDefined(), "If the code is not defined an exception must be passed in explaining why.");
            Debug.Assert(code != EOperationCode.Invalid, "If the code is invalid an exception must be passed in explaining why.");

            Instruction = instruction;
            File = file;
            MIPSStringDelegate = toMIPSString;
        }

        internal Operation(EOperationCode code, in Instruction instruction, DataFile file, ToMIPSStringDelegate toMIPSString, Exception e)
            : base(code)
        {
            Instruction = instruction;
            File = file;
            MIPSStringDelegate = toMIPSString;
            Exception = e;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Operation NotImplemented(EOperationCode code, in Instruction instruction, DataFile file)
        {
            return new Operation(code, instruction, file, static (string name, in Instruction _) =>
                $"/* {name} NOT IMPLEMENTED */");
        }

        /// <summary>
        /// Returns this operation as a MIPS instruction.
        /// </summary>
        public string GetMIPSString()
        {
            return MIPSStringDelegate(Name, Instruction);
        }
    }
}
