using System;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Core.Reflection.Emit
{
    /// <summary>
    /// <see cref="ILGenerator"/> extensions.
    /// </summary>
    public static class ILGeneratorExtensions
    {
        /// <summary>
        /// Emits IL for a call to the provided <see cref="ConstructorInfo"/>.
        /// </summary>
        /// <param name="il">The <see cref="ILGenerator"/>.</param>
        /// <param name="method">The method to invoke.</param>
        public static void EmitCall(this ILGenerator il, ConstructorInfo method)
        {
            Guard.IsNotNull(il, nameof(il));
            Guard.IsNotNull(method, nameof(method));

            if (method.IsStatic)
            {
                il.Emit(OpCodes.Call, method);
            }
            else
            {
                il.Emit(OpCodes.Callvirt, method);
            }
        }

        /// <summary>
        /// Emits IL for a call to the provided <see cref="MethodInfo"/>.
        /// </summary>
        /// <param name="il">The <see cref="ILGenerator"/>.</param>
        /// <param name="method">The method to invoke.</param>
        public static void EmitCall(this ILGenerator il, MethodInfo method)
        {
            Guard.IsNotNull(il, nameof(il));
            Guard.IsNotNull(method, nameof(method));

            if (method.IsStatic)
            {
                il.Emit(OpCodes.Call, method);
            }
            else
            {
                il.Emit(OpCodes.Callvirt, method);
            }
        }

        /// <summary>
        /// Puts the appropriate <see langword="ldarg"/> instruction to read an argument onto the stream of instructions.
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="index">The index of the argument to load</param>
        public static void EmitLoadArgument(this ILGenerator il, int index)
        {
            Guard.IsNotNull(il, nameof(il));

            if (index <= 3)
            {
                il.Emit(index switch
                {
                    0 => OpCodes.Ldarg_0,
                    1 => OpCodes.Ldarg_1,
                    2 => OpCodes.Ldarg_2,
                    3 => OpCodes.Ldarg_3,
                    _ => throw new InvalidOperationException($"Invalid argument index [{index}]")
                });
            }
            else if (index <= 255)
            {
                il.Emit(OpCodes.Ldarg_S, (byte)index);
            }
            else if (index <= 65534)
            {
                il.Emit(OpCodes.Ldarg, (short)index);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Invalid argument index {index}");
            }
        }

        /// <summary>
        /// Puts the appropriate <see langword="ldind"/> or <see langword="ldobj"/> instruction to read from a reference onto the stream of instructions
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="type">The type of value being read from the current reference on top of the execution stack</param>
        public static void EmitLoadReference(this ILGenerator il, Type type)
        {
            Guard.IsNotNull(il, nameof(il));
            Guard.IsNotNull(type, nameof(type));

            if (type.IsValueType)
            {
                // Pick the optimal opcode to set a value type
                // We don't have access to this right now, so
                // we will opt for the slower instruction.
                //  OpCode opcode = type.GetSize() switch
                //  {
                //	    // Use the faster op codes for sizes <= 8
                //  	1 when type == typeof(bool) || type == typeof(byte) => OpCodes.Ldind_U1,
                //		1 when type == typeof(sbyte) => OpCodes.Ldind_I1,
                //		2 when type == typeof(short) => OpCodes.Ldind_I2,
                //		2 when type == typeof(ushort) => OpCodes.Ldind_U2,
                //		4 when type == typeof(float) => OpCodes.Ldind_R4,
                //		4 when type == typeof(int) => OpCodes.Ldind_I4,
                //		4 when type == typeof(uint) => OpCodes.Ldind_U4,
                //		8 when type == typeof(double) => OpCodes.Ldind_R8,
                //		8 when type == typeof(long) || type == typeof(ulong) => OpCodes.Ldind_I8,
                //
                //		// Default to ldobj for all other value types
                //		_ => OpCodes.Ldobj
                //	};
                var opcode = OpCodes.Ldobj;

                // Also pass the type token if ldobj is used
                if (opcode == OpCodes.Ldobj)
                {
                    il.Emit(opcode, type);
                }
                else
                {
                    il.Emit(opcode);
                }
            }
            else
            {
                il.Emit(OpCodes.Ldind_Ref);
            }
        }

        /// <summary>
        /// Puts the <see langword="ret"/> opcode onto the stream of instructions.
        /// </summary>
        /// <param name="il"></param>
        public static void EmitReturn(this ILGenerator il)
        {
            Guard.IsNotNull(il, nameof(il));

            il.Emit(OpCodes.Ret);
        }
    }
}
