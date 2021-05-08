using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Foundry.Media.Nintendo64.Rom.Disassembly.Internal;
using Foundry.Linq;
using Foundry.Reflection;

namespace Foundry.Media.Nintendo64.Rom.Disassembly
{
    /// <summary>
    /// Metadata about a MIPS operation.
    /// </summary>
    public class OperationMetadata
    {
        private static IImmutableDictionary<EOperationCode, OperationMetadata> Cache;

        /// <summary>
        /// The operation code.
        /// </summary>
        public EOperationCode Code { get; }

        /// <summary>
        /// The operation code instruction.
        /// </summary>
        public string Name => Code.GetMember().GetAttribute<OperationAttribute>()?.Instruction ?? Code.ToString().ToLower().Replace('_', '.');

        /// <summary>
        /// The descriptive name of the operation.
        /// </summary>
        public string DescriptiveName => Code.GetMember().GetAttribute<OperationAttribute>()!.Name;

        /// <summary>
        /// A string representing the argument list.
        /// </summary>
        public string Arguments => Code.GetMember().GetAttribute<OperationAttribute>()!.Arguments;

        /// <summary>
        /// A very short summary of what this operation does.
        /// </summary>
        public string Summary => Code.GetMember().GetAttribute<OperationAttribute>()!.Summary;

        /// <summary>
        /// A long description of what this operation does.
        /// </summary>
        public string Description => Code.GetMember().GetAttribute<DescriptionAttribute>()!.Description;

        /// <summary>
        /// A list of restrictions for this operation.
        /// </summary>
        public IImmutableList<string> Restrictions => Code.GetMember().GetAttributes<RestrictionAttribute>().Select(r => r.Restriction).ToImmutableArray();

        /// <summary>
        /// Any misnomers relating to the naming of this operation.
        /// </summary>
        public string? Misnomer => Code.GetMember().GetAttribute<OperationAttribute>()!.Misnomer;

        /// <summary>
        /// The MIPS version this operation was introduced in.
        /// </summary>
        public EMipsVersion IntroducedIn => Code.GetMember().GetAttribute<OperationAttribute>()!.IntroducedIn;

        private protected OperationMetadata(EOperationCode code)
        {
            Code = code;
        }

        static OperationMetadata()
        {
            //TODO: Move to unit tests.
            //foreach (var value in Enums.GetMembers<EOperationCode>())
            //{
            //    var opAttr = value.GetAttribute<OperationAttribute>();
            //    if (opAttr is null)
            //    {
            //        throw new Exception($"Missing {nameof(OperationAttribute)} on enum value {value.Name}.");
            //    }

            //    string expectedName = value.Name.Replace('_', '.').ToLower();
            //    if (!opAttr.Instruction.Equals(expectedName, StringComparison.OrdinalIgnoreCase))
            //    {
            //        throw new Exception($"Enum value {value.Name} expected instruction {expectedName}, got {opAttr.Instruction}");
            //    }
            //}

            Cache = Enums.GetValues<EOperationCode>().ToImmutableDictionary(o => o, o => new OperationMetadata(o));
        }

        /// <summary>
        /// Attempts to get the metadata for an operation.
        /// </summary>
        /// <param name="operationName">The name of the operation to get the metadata for.</param>
        /// <param name="result">On success, this value will contain the metadata for operation.</param>
        public static bool TryGetMetadata([NotNullWhen(true)] string? operationName, [NotNullWhen(true)] out OperationMetadata? result)
        {
            result = null;

            if (string.IsNullOrWhiteSpace(operationName))
            {
                return false;
            }

            if (Enums.TryParse(operationName, true, out EOperationCode code))
            {
                return Cache.TryGetValue(code, out result);
            }

            return false;
        }

        /// <summary>
        /// Attempts to get the metadata for an operation.
        /// </summary>
        /// <param name="operationName">The name of the operation to get the metadata for.</param>
        /// <param name="result">On success, this value will contain the metadata for operation.</param>
        public static bool TryGetMetadata(in ReadOnlySpan<char> operationName, [NotNullWhen(true)] out OperationMetadata? result)
        {
            string n = operationName.IsEmpty
                ? string.Empty
                : new string(operationName);
            return TryGetMetadata(n, out result);
        }

        /// <summary>
        /// Returns a collection of operations grouped by the version of MIPS they were introduced in.
        /// </summary>
        public static IEnumerable<IGrouping<string, OperationMetadata>> GetOperationsByVersion()
        {
            return Cache
                .Values
                .GroupBy(o => o.IntroducedIn, o => o)
                .OrderBy(o => o.Key)
                .TransformKey(o => o.GetDescription()!);
        }
    }
}
