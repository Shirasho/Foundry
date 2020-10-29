using System;
using System.Linq;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    public static class ActionHelper
    {
        /// <summary>
        /// Creates an <see cref="Action{T}"/> delegate type using the specified parameter type.
        /// </summary>
        /// <param name="paramType">The parameter type of the action to create.</param>
        public static Type GetActionType(Type paramType)
        {
            // Unfortunately this will allocate an array instance anyway, but we will
            // have this override just in case MakeGenericType gets a single Type
            // override.
            return typeof(Action<>).MakeGenericType(paramType);
        }

        /// <summary>
        /// Creates an <see cref="Action"/> delegate type using the specified parameter types.
        /// </summary>
        /// <param name="paramTypes">The parameter types of the action to create.</param>
        public static Type GetActionType(params Type[]? paramTypes)
            => GetFuncOrActionType(paramTypes, typeof(void));

        /// <summary>
        /// Creates an open generic <see cref="Action"/> delegate type with the specified number of arguments.
        /// </summary>
        /// <param name="argumentCount">The number of expected generic arguments.</param>
        /// <exception cref="NotSupportedException"><paramref name="argumentCount"/> is greater than 7.</exception>
        public static Type GetActionType(int argumentCount)
            => GetFuncOrActionType(argumentCount, typeof(void));

        /// <summary>
        /// Creates an open generic <see cref="Action"/> or <see cref="Func{T, TResult}"/> type with the specified number of arguments.
        /// </summary>
        /// <param name="argumentCount">The number of expected generic arguments.</param>
        /// <param name="returnType">The return type of the <see cref="Func{T, TResult}"/>, or <see langword="null"/> to create an <see cref="Action"/> type.</param>
        /// <exception cref="NotSupportedException"><paramref name="argumentCount"/> is greater than 7.</exception>
        public static Type GetFuncOrActionType(int argumentCount, Type? returnType)
        {
            Guard.IsNotNull(returnType, nameof(returnType));

            if (returnType == typeof(void) || returnType is null)
            {
                return argumentCount switch
                {
                    0 => typeof(Action),
                    1 => typeof(Action<>),
                    2 => typeof(Action<,>),
                    3 => typeof(Action<,,>),
                    4 => typeof(Action<,,,>),
                    5 => typeof(Action<,,,,>),
                    6 => typeof(Action<,,,,,>),
                    7 => typeof(Action<,,,,,,>),
                    _ => throw new NotSupportedException($"Action with so many ({argumentCount}) parameters is not supported!"),
                };
            }

            return argumentCount switch
            {
                0 => typeof(Func<>),
                1 => typeof(Func<,>),
                2 => typeof(Func<,,>),
                3 => typeof(Func<,,,>),
                4 => typeof(Func<,,,,>),
                5 => typeof(Func<,,,,,>),
                6 => typeof(Func<,,,,,,>),
                7 => typeof(Func<,,,,,,,>),
                _ => throw new NotSupportedException($"Func with so many ({argumentCount}) parameters is not supported!"),
            };
        }

        /// <summary>
        /// Creates an open generic <see cref="Action"/> or <see cref="Func{T, TResult}"/> type with the specified parameter types
        /// and return type.
        /// </summary>
        /// <param name="paramTypes">The parameter types of the action to create.</param>
        /// <param name="returnType">The return type of the <see cref="Func{T, TResult}"/>, or <see langword="null"/> to create an <see cref="Action"/> type.</param>
        /// <exception cref="NotSupportedException"><paramref name="argumentCount"/> is greater than 7.</exception>
        public static Type GetFuncOrActionType(Type[]? paramTypes, Type? returnType)
        {
            if (returnType == typeof(void) || returnType is null)
            {
                return GetFuncOrActionType(paramTypes?.Length ?? 0, returnType).MakeGenericType(paramTypes);
            }

            return GetFuncOrActionType(paramTypes?.Length ?? 0, returnType).MakeGenericType((paramTypes ?? Array.Empty<Type>()).Concat(new[] { returnType }).ToArray());
        }
    }
}
