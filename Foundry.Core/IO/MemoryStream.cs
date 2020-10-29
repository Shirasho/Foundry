using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.IO
{
    /// <summary>
    /// A <see cref="Stream"/> that wraps a <see cref="Memory{T}"/> or <see cref="ReadOnlyMemory{T}"/>
    /// instance.
    /// </summary>
    public class MemoryStream : Stream
    {
        /// <summary>
        /// Whether this <see cref="MemoryStream"/> has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns><see langword="true" /> if the stream supports reading; otherwise, <see langword="false" />.</returns>
        public sealed override bool CanRead
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !IsDisposed;
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns><see langword="true" /> if the stream supports seeking; otherwise, <see langword="false" />.</returns>
        public override bool CanSeek
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !IsDisposed;
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns><see langword="true" /> if the stream supports writing; otherwise, <see langword="false" />.</returns>
        public override bool CanWrite
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !IsReadOnlyMemory && !IsDisposed;
        }

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <returns>A long value representing the length of the stream in bytes.</returns>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public override long Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckDisposed();

                return Memory.Length;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <returns>The current position within the stream.</returns>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public override long Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckDisposed();

                return InternalPosition;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                CheckDisposed();
                CheckPosition(value, Memory.Length);

                InternalPosition = unchecked((int)value);
            }
        }

        /// <summary>
        /// Whether <see cref="Memory"/> was originally a
        /// <see cref="ReadOnlyMemory{T}"/> instance.
        /// </summary>
        private readonly bool IsReadOnlyMemory;

        /// <summary>
        /// The memory we are tracking.
        /// </summary>
        private Memory<byte> Memory;

        /// <summary>
        /// The index of the current position pointer.
        /// </summary>
        private int InternalPosition;

        public MemoryStream(in Memory<byte> memory)
        {
            Memory = memory;
            Position = 0;
            IsReadOnlyMemory = false;
        }

        public MemoryStream(in ReadOnlyMemory<byte> memory)
        {
            Memory = MemoryMarshal.AsMemory(memory);
            Position = 0;
            IsReadOnlyMemory = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            Memory = default;
            IsDisposed = true;

            base.Dispose(disposing);
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public sealed override long Seek(long offset, SeekOrigin origin)
        {
            CheckDisposed();

            long index = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => Position + offset,
                SeekOrigin.End => Length + offset,
                _ => throw new ArgumentException()
            };

            CheckPosition(index, Memory.Length);

            InternalPosition = unchecked((int)index);

            return index;
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public sealed override void Flush()
        {

        }

        /// <summary>
        /// Asynchronously clears all buffers for this stream, causes any buffered data to be written to the underlying device, and monitors cancellation requests.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        /// <returns>A task that represents the asynchronous flush operation.</returns>
        public sealed override Task FlushAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Reads the bytes from the current stream and writes them to another stream, using a specified buffer size.
        /// </summary>
        /// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
        /// <param name="bufferSize">The size of the buffer. This value must be greater than zero. The default size is 81920.</param>
        /// <exception cref="ArgumentNullException"><paramref name="destination" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferSize" /> is negative or zero.</exception>
        /// <exception cref="NotSupportedException">The current stream does not support reading.
        ///  -or-
        ///  <paramref name="destination" /> does not support writing.</exception>
        /// <exception cref="ObjectDisposedException">Either the current stream or <paramref name="destination" /> were closed before the <see cref="M:System.IO.Stream.CopyTo(System.IO.Stream)" /> method was called.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        [SuppressMessage("Usage", "PC001:API not supported on all platforms", Justification = "False positive.")]
        public sealed override void CopyTo(Stream destination, int bufferSize)
        {
            CheckDisposed();

            var source = Memory.Span.Slice(InternalPosition);

            InternalPosition += source.Length;

            destination.Write(source);
        }

        /// <summary>
        /// Asynchronously reads the bytes from the current stream and writes them to another stream, using a specified buffer size and cancellation token.
        /// </summary>
        /// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
        /// <param name="bufferSize">The size, in bytes, of the buffer. This value must be greater than zero. The default size is 81920.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        /// <returns>A task that represents the asynchronous copy operation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="destination" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="buffersize" /> is negative or zero.</exception>
        /// <exception cref="ObjectDisposedException">Either the current stream or the destination stream is disposed.</exception>
        /// <exception cref="NotSupportedException">The current stream does not support reading, or the destination stream does not support writing.</exception>
        public sealed override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            // Span does not have async support for stuff.
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            try
            {
                CopyTo(destination, bufferSize);

                return Task.CompletedTask;
            }
            catch (OperationCanceledException e)
            {
                return Task.FromCanceled(e.CancellationToken);
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }

        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns>The unsigned byte cast to an <see langword="Int32" />, or -1 if at the end of the stream.</returns>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public sealed override int ReadByte()
        {
            CheckDisposed();

            if (InternalPosition == Memory.Length)
            {
                return -1;
            }

            return Memory.Span[InternalPosition++];
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        /// <exception cref="ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        [SuppressMessage("Usage", "PC001:API not supported on all platforms", Justification = "False positive.")]
        public sealed override int Read(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            CheckBuffer(buffer, offset, count);

            int bytesAvailable = Memory.Length - InternalPosition;
            int bytesCopied = Math.Min(bytesAvailable, count);

            var source = Memory.Span.Slice(InternalPosition, bytesCopied);
            var dest = buffer.AsSpan(offset, bytesCopied);

            source.CopyTo(dest);

            InternalPosition += bytesCopied;

            return bytesCopied;
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">A region of memory. When this method returns, the contents of this region are replaced by the bytes read from the current source.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes allocated in the buffer if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        [SuppressMessage("Usage", "PC001:API not supported on all platforms", Justification = "False positive.")]
        public sealed override int Read(Span<byte> buffer)
        {
            CheckDisposed();

            int bytesAvailable = Memory.Length - InternalPosition;
            int bytesCopied = Math.Min(bytesAvailable, buffer.Length);

            var source = Memory.Span.Slice(InternalPosition, bytesCopied);

            source.CopyTo(buffer);

            InternalPosition += bytesCopied;

            return bytesCopied;
        }

        /// <summary>
        /// Asynchronously reads a sequence of bytes from the current stream, advances the position within the stream by the number of bytes read, and monitors cancellation requests.
        /// </summary>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin writing data from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        /// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
        /// <exception cref="ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length.</exception>
        /// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
        public sealed override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            // Span does not have async support for stuff.
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<int>(cancellationToken);
            }

            try
            {
                int result = Read(buffer, offset, count);

                return Task.FromResult(result);
            }
            catch (OperationCanceledException e)
            {
                return Task.FromCanceled<int>(e.CancellationToken);
            }
            catch (Exception e)
            {
                return Task.FromException<int>(e);
            }
        }

        /// <summary>
        /// Asynchronously reads a sequence of bytes from the current stream, advances the position within the stream by the number of bytes read, and monitors cancellation requests.
        /// </summary>
        /// <param name="buffer">The region of memory to write the data into.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        /// <returns>A task that represents the asynchronous read operation. The value of its <see cref="P:System.Threading.Tasks.ValueTask`1.Result" /> property contains the total number of bytes read into the buffer. The result value can be less than the number of bytes allocated in the buffer if that many bytes are not currently available, or it can be 0 (zero) if the end of the stream has been reached.</returns>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            // Span does not have async support for stuff.
            if (cancellationToken.IsCancellationRequested)
            {
                return new ValueTask<int>(Task.FromCanceled<int>(cancellationToken));
            }

            try
            {
                int result = Read(buffer.Span);

                return new ValueTask<int>(Task.FromResult(result));
            }
            catch (OperationCanceledException e)
            {
                return new ValueTask<int>(Task.FromCanceled<int>(e.CancellationToken));
            }
            catch (Exception e)
            {
                return new ValueTask<int>(Task.FromException<int>(e));
            }
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is greater than the buffer length.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
        /// <exception cref="NotSupportedException">The stream does not support writing.</exception>
        /// <exception cref="ObjectDisposedException"><see cref="M:System.IO.Stream.Write(System.Byte[],System.Int32,System.Int32)" /> was called after the stream was closed.</exception>
        [SuppressMessage("Usage", "PC001:API not supported on all platforms", Justification = "False positive.")]
        public sealed override void Write(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            if (!CanWrite)
            {
                throw new NotSupportedException();
            }
            CheckBuffer(buffer, offset, count);

            var source = buffer.AsSpan(offset, count);
            var dest = Memory.Span.Slice(InternalPosition);

            if (!source.TryCopyTo(dest))
            {
                throw new ArgumentException("The current stream cannot contain the requested input data.");
            }

            InternalPosition += source.Length;
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">A region of memory. This method copies the contents of this region to the current stream.</param>
        [SuppressMessage("Usage", "PC001:API not supported on all platforms", Justification = "False positive.")]
        public sealed override void Write(ReadOnlySpan<byte> buffer)
        {
            CheckDisposed();
            if (!CanWrite)
            {
                throw new NotSupportedException();
            }

            var dest = Memory.Span.Slice(InternalPosition);

            if (!buffer.TryCopyTo(dest))
            {
                throw new ArgumentException("The current stream cannot contain the requested input data.");
            }

            InternalPosition += buffer.Length;
        }

        /// <summary>
        /// Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream by the number of bytes written, and monitors cancellation requests.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> from which to begin copying bytes to the stream.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
        /// <exception cref="ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length.</exception>
        /// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
        public sealed override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            // Span does not have async support for stuff.
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            try
            {
                Write(buffer, offset, count);

                return Task.CompletedTask;
            }
            catch (OperationCanceledException e)
            {
                return Task.FromCanceled(e.CancellationToken);
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }

        /// <summary>
        /// Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream by the number of bytes written, and monitors cancellation requests.
        /// </summary>
        /// <param name="buffer">The region of memory to write data from.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public sealed override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            // Span does not have async support for stuff.
            if (cancellationToken.IsCancellationRequested)
            {
                return new ValueTask(Task.FromCanceled(cancellationToken));
            }

            try
            {
                Write(buffer.Span);

                return new ValueTask();
            }
            catch (OperationCanceledException e)
            {
                return new ValueTask(Task.FromCanceled(e.CancellationToken));
            }
            catch (Exception e)
            {
                return new ValueTask(Task.FromException(e));
            }
        }

        public override void WriteByte(byte value)
        {
            CheckDisposed();
            if (!CanWrite)
            {
                throw new NotSupportedException();
            }

            if (InternalPosition == Memory.Length)
            {
                throw new ArgumentException("The current stream cannot contain the requested input data.");
            }

            Memory.Span[InternalPosition++] = value;
        }

        public sealed override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckDisposed()
        {
            if (IsDisposed)
            {
                ThrowDisposedException();
            }
        }

        [DoesNotReturn]
        private void ThrowDisposedException()
        {
            throw new ObjectDisposedException(nameof(MemoryStream));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckPosition(long position, int length)
        {
            if ((ulong)position >= (ulong)length)
            {
                ThrowInvalidRangeException(nameof(position));
            }
        }

        [DoesNotReturn]
        private static void ThrowInvalidRangeException(string argumentName)
        {
            throw new ArgumentOutOfRangeException(argumentName, "The value for the position is not in a valid range.");
        }

        private static void CheckBuffer(byte[]? buffer, int offset, int count)
        {
            Guard.IsNotNull(buffer, nameof(buffer));
            Guard.IsGreaterThanOrEqualTo(offset, 0, nameof(offset));
            Guard.IsGreaterThanOrEqualTo(count, 0, nameof(count));

            if (offset + count > buffer.Length)
            {
                throw new ArgumentException($"The sum of {nameof(offset)} and {nameof(count)} can't be larger than the buffer length.", nameof(buffer));
            }
        }
    }
}
