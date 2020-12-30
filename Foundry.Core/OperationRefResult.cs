using System;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    /// <summary>
    /// Defines an operation result.
    /// </summary>
    public readonly ref struct OperationRefResult
    {
        /// <summary>
        /// Whether the operation completed successfully.
        /// </summary>
        public readonly bool Success { get; }

        /// <summary>
        /// The exception that occurred while executing the operation.
        /// </summary>
        public readonly Exception? Exception { get; }

        /// <summary>
        /// A message describing the exception.
        /// </summary>
        /// <remarks>
        /// This can be used as a replacement for <see cref="Exception"/>
        /// if creating an exception is not desired.
        /// </remarks>
        public string? ExceptionMessage { get; }

        public OperationRefResult(bool success)
        {
            Guard.IsTrue(success, nameof(success));

            Success = success;
            Exception = null;
            ExceptionMessage = null;
        }

        public OperationRefResult(Exception e)
        {

            Guard.IsNotNull(e, nameof(e));

            Success = false;
            Exception = e;
            ExceptionMessage = e.Message;
        }

        public OperationRefResult(string errorMessage)
        {
            Guard.IsNotNullOrWhitespace(errorMessage, nameof(errorMessage));

            Success = false;
            Exception = null;
            ExceptionMessage = errorMessage;
        }
    }

    /// <summary>
    /// Defines an operation result that returns <typeparamref name="T"/>
    /// on success.
    /// </summary>
    /// <typeparam name="T">The type that this operation should return on success.</typeparam>
    public readonly ref struct OperationRefResult<T>
    {
        /// <summary>
        /// Whether the operation completed successfully.
        /// </summary>
        public readonly bool Success { get; }

        /// <summary>
        /// The exception that occurred while executing the operation.
        /// </summary>
        public readonly Exception? Exception { get; }

        /// <summary>
        /// A message describing the exception.
        /// </summary>
        /// <remarks>
        /// This can be used as a replacement for <see cref="Exception"/>
        /// if creating an exception is not desired.
        /// </remarks>
        public string? ExceptionMessage { get; }

        /// <summary>
        /// The result of this operation if <see cref="OperationRefResult.Success"/>
        /// is <see langword="true"/>.
        /// </summary>
        public T? Result { get; }

        public OperationRefResult(T? result)
        {
            Success = true;
            Result = result;
            Exception = null;
            ExceptionMessage = null;
        }

        public OperationRefResult(Exception e)
        {

            Guard.IsNotNull(e, nameof(e));

            Success = false;
            Result = default;
            Exception = e;
            ExceptionMessage = e.Message;
        }

        public OperationRefResult(string errorMessage)
        {
            Guard.IsNotNullOrWhitespace(errorMessage, nameof(errorMessage));

            Success = false;
            Result = default;
            Exception = null;
            ExceptionMessage = errorMessage;
        }
    }

    /// <summary>
    /// Defines an operation result that returns <typeparamref name="T1"/>
    /// and <typeparamref name="T2"/> on success.
    /// </summary>
    /// <typeparam name="T1">The first type that this operation should return on success.</typeparam>
    /// <typeparam name="T2">The second type that this operation should return on success.</typeparam>
    public readonly ref struct OperationRefResult<T1, T2>
    {
        /// <summary>
        /// Whether the operation completed successfully.
        /// </summary>
        public readonly bool Success { get; }

        /// <summary>
        /// The exception that occurred while executing the operation.
        /// </summary>
        public readonly Exception? Exception { get; }

        /// <summary>
        /// A message describing the exception.
        /// </summary>
        /// <remarks>
        /// This can be used as a replacement for <see cref="Exception"/>
        /// if creating an exception is not desired.
        /// </remarks>
        public string? ExceptionMessage { get; }

        /// <summary>
        /// The first result of this operation if <see cref="OperationRefResult.Success"/>
        /// is <see langword="true"/>.
        /// </summary>
        public T1? Result1 { get; }

        /// <summary>
        /// The second result of this operation if <see cref="OperationRefResult.Success"/>
        /// is <see langword="true"/>.
        /// </summary>
        public T2? Result2 { get; }

        public OperationRefResult(T1? result1, T2? result2)
        {
            Success = true;
            Result1 = result1;
            Result2 = result2;
            Exception = null;
            ExceptionMessage = null;
        }

        public OperationRefResult(Exception e)
        {
            Guard.IsNotNull(e, nameof(e));

            Success = false;
            Result1 = default;
            Result2 = default;
            Exception = e;
            ExceptionMessage = e.Message;
        }

        public OperationRefResult(string errorMessage)
        {
            Guard.IsNotNullOrWhitespace(errorMessage, nameof(errorMessage));

            Success = false;
            Result1 = default;
            Result2 = default;
            Exception = null;
            ExceptionMessage = errorMessage;
        }
    }
}
