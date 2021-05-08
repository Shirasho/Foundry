using System;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    /// <summary>
    /// The level of an <see cref="OperationResult"/>'s level.
    /// </summary>
    public enum EOperationResultLevel
    {
        Success,
        Warning,
        Error
    }

    /// <summary>
    /// Defines an operation result.
    /// </summary>
    public class OperationResult
    {
        /// <summary>
        /// Whether the operation completed successfully.
        /// </summary>
        public bool Success => Level == EOperationResultLevel.Success;

        /// <summary>
        /// The result level.
        /// </summary>
        public EOperationResultLevel Level { get; }

        /// <summary>
        /// The exception that occurred while executing the operation.
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// A message describing the exception.
        /// </summary>
        /// <remarks>
        /// This can be used as a replacement for <see cref="Exception"/>
        /// if creating an exception is not desired.
        /// </remarks>
        public string? ExceptionMessage { get; }

        public OperationResult()
        {
            Level = EOperationResultLevel.Success;
        }

        public OperationResult(Exception e)
            : this(e, EOperationResultLevel.Error)
        {

        }

        public OperationResult(Exception e, EOperationResultLevel level)
        {
            Guard.IsNotNull(e, nameof(e));
            GuardEx.IsValid(level, nameof(level));

            Level = level;
            Exception = e;
            ExceptionMessage = e.Message;
        }

        public OperationResult(string errorMessage)
            : this(errorMessage, EOperationResultLevel.Error)
        {

        }

        public OperationResult(string errorMessage, EOperationResultLevel level)
        {
            Guard.IsNotNullOrWhiteSpace(errorMessage, nameof(errorMessage));
            GuardEx.IsValid(level, nameof(level));

            Level = level;
            ExceptionMessage = errorMessage;
        }
    }

    /// <summary>
    /// Defines an operation result that returns <typeparamref name="T"/>
    /// on success.
    /// </summary>
    /// <typeparam name="T">The type that this operation should return on success.</typeparam>
    public class OperationResult<T> : OperationResult
    {
        /// <summary>
        /// The result of this operation if <see cref="OperationResult.Success"/>
        /// is <see langword="true"/>.
        /// </summary>
        public T? Result { get; }

        public OperationResult(T? result)
        {
            Result = result;
        }

        public OperationResult(Exception e)
            : base(e)
        {
        }

        public OperationResult(string errorMessage)
            : base(errorMessage)
        {

        }
    }

    /// <summary>
    /// Defines an operation result that returns <typeparamref name="T1"/>
    /// and <typeparamref name="T2"/> on success.
    /// </summary>
    /// <typeparam name="T1">The first type that this operation should return on success.</typeparam>
    /// <typeparam name="T2">The second type that this operation should return on success.</typeparam>
    public class OperationResult<T1, T2> : OperationResult
    {
        /// <summary>
        /// The first result of this operation if <see cref="OperationResult.Success"/>
        /// is <see langword="true"/>.
        /// </summary>
        public T1? Result1 { get; }

        /// <summary>
        /// The second result of this operation if <see cref="OperationResult.Success"/>
        /// is <see langword="true"/>.
        /// </summary>
        public T2? Result2 { get; }

        public OperationResult(T1? result1, T2? result2)
        {
            Result1 = result1;
            Result2 = result2;
        }

        public OperationResult(Exception e)
            : base(e)
        {
        }

        public OperationResult(string errorMessage)
            : base(errorMessage)
        {

        }
    }
}
