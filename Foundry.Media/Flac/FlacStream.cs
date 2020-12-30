using System;
using System.IO;
using Microsoft.Toolkit.Diagnostics;

using MemoryStream = Foundry.IO.MemoryStream;

namespace Foundry.Media.Flac
{
    /// <summary>
    /// A FLAC stream.
    /// </summary>
    public class FlacStream : Stream
    {
        /// <summary>
        /// The metadata for this FLAC data.
        /// </summary>
        public FlacMetadata Metadata { get; private set; } = FlacMetadata.Empty;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>true if the stream supports reading; otherwise, false.</returns>
        public override bool CanRead => UnderlyingStream.CanRead;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>true if the stream supports seeking; otherwise, false.</returns>
        public override bool CanSeek => UnderlyingStream.CanSeek;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>true if the stream supports writing; otherwise, false.</returns>
        public override bool CanWrite => false;

        public override long Length => UnderlyingStream.Length;

        public override long Position { get => UnderlyingStream.Position; set => UnderlyingStream.Position = value; }

        private readonly Stream UnderlyingStream;
        private readonly int DataOffset;

        public FlacStream(Stream stream)
        {
            Guard.IsNotNull(stream, nameof(stream));
            Guard.CanRead(stream, nameof(stream));

            UnderlyingStream = stream;
            ReadMetadata();

            DataOffset = Metadata.DataOffset;
        }

        public FlacStream(ReadOnlyMemory<byte> data)
            : this(new MemoryStream(data))
        {

        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        public override void Flush()
        {

        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        /// <exception cref="ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length.</exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="buffer" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public override int Read(byte[] buffer, int offset, int count)
            => UnderlyingStream.Read(buffer, offset, count);

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return origin switch
            {
                SeekOrigin.Begin => UnderlyingStream.Seek(DataOffset + offset, origin),
                _ => UnderlyingStream.Seek(DataOffset, origin)
            };
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is greater than the buffer length.</exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="buffer" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
        /// <exception cref="IOException">An I/O error occurred, such as the specified file cannot be found.</exception>
        /// <exception cref="NotSupportedException">The stream does not support writing.</exception>
        /// <exception cref="ObjectDisposedException">
        ///   <see cref="M:System.IO.Stream.Write(System.Byte[],System.Int32,System.Int32)" /> was called after the stream was closed.</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        private void ReadMetadata()
        {
            // Relying on an optimization by the JIT to push this data into metadata
            // storage and return a ref here.
            Span<byte> expectedSpecificationBytes = new byte[] { 0x66, 0x4C, 0x61, 0x43 };

            // Read the first bytes of the stream to ensure this stream is expected a FLAC stream.
            Span<byte> actualSpecificationBytes = stackalloc byte[expectedSpecificationBytes.Length];

            int specificationBytesRead = UnderlyingStream.Read(actualSpecificationBytes);
            if (specificationBytesRead != actualSpecificationBytes.Length || !expectedSpecificationBytes.SequenceEqual(actualSpecificationBytes))
            {
                throw new FlacException("The stream does not represent valid FLAC data.");
            }

            Metadata = FlacMetadata.Create(expectedSpecificationBytes.Length, UnderlyingStream);
        }
    }
}
