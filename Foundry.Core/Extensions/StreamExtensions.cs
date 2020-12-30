using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    public static class StreamExtensions
    {
        /// <summary>
        /// The default buffer size, in bytes.
        /// </summary>
        public const int DefaultStreamBufferSize = 4096;

        /// <summary>
        /// Copies the contents of <paramref name="source"/> to <paramref name="destination"/>.
        /// </summary>
        /// <param name="source">The source <see cref="Stream"/>.</param>
        /// <param name="destination">The destination <see cref="Stream"/>.</param>
        /// <param name="bufferSize">The buffer size.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static Task CopyToAsync(this Stream source, Stream destination, int bufferSize, CancellationToken cancellationToken = default)
            => CopyToAsync(source, destination, bufferSize, null, cancellationToken);

        /// <summary>
        /// Copies the contents of <paramref name="source"/> to <paramref name="destination"/>.
        /// </summary>
        /// <param name="source">The source <see cref="Stream"/>.</param>
        /// <param name="destination">The destination <see cref="Stream"/>.</param>
        /// <param name="progress">The progress provider.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static Task CopyToAsync(this Stream source, Stream destination, IProgress<long>? progress, CancellationToken cancellationToken = default)
            => CopyToAsync(source, destination, DefaultStreamBufferSize, progress, cancellationToken);

        /// <summary>
        /// Copies the contents of <paramref name="source"/> to <paramref name="destination"/>.
        /// </summary>
        /// <param name="source">The source <see cref="Stream"/>.</param>
        /// <param name="destination">The destination <see cref="Stream"/>.</param>
        /// <param name="bufferSize">The buffer size.</param>
        /// <param name="progress">The progress provider.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long>? progress, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(source, nameof(source));
            Guard.IsNotNull(destination, nameof(destination));
            Guard.IsGreaterThanOrEqualTo(bufferSize, 1, nameof(bufferSize));

            byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                long totalBytesRead = 0;
                int bytesRead;
                while ((bytesRead = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false)) != 0)
                {
                    await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
                    totalBytesRead += bytesRead;
                    progress?.Report(totalBytesRead);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Returns a <see cref="StreamReader"/> for the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to get the reader for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StreamReader GetStreamReader(this Stream stream)
            => GetStreamReader(stream, Encoding.UTF8, DefaultStreamBufferSize);

        /// <summary>
        /// Returns a <see cref="StreamReader"/> for the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to get the reader for.</param>
        /// <param name="encoding">The encoding to use while reading the stream.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StreamReader GetStreamReader(this Stream stream, Encoding? encoding)
            => GetStreamReader(stream, encoding, DefaultStreamBufferSize);

        /// <summary>
        /// Returns a <see cref="StreamReader"/> for the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to get the reader for.</param>
        /// <param name="leaveOpen">Whether to leave the stream open when the generated <see cref="StreamReader"/> is disposed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StreamReader GetStreamReader(this Stream stream, bool leaveOpen)
            => GetStreamReader(stream, null, DefaultStreamBufferSize, leaveOpen);

        /// <summary>
        /// Returns a <see cref="StreamReader"/> for the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to get the reader for.</param>
        /// <param name="bufferSize">The buffer size.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StreamReader GetStreamReader(this Stream stream, int bufferSize)
            => GetStreamReader(stream, Encoding.UTF8, bufferSize);

        /// <summary>
        /// Returns a <see cref="StreamReader"/> for the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to get the reader for.</param>
        /// <param name="encoding">The encoding to use while reading the stream.</param>
        /// <param name="bufferSize">The buffer size.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StreamReader GetStreamReader(this Stream stream, Encoding? encoding, int bufferSize)
            => GetStreamReader(stream, encoding, bufferSize, true);

        /// <summary>
        /// Returns a <see cref="StreamReader"/> for the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to get the reader for.</param>
        /// <param name="encoding">The encoding to use while reading the stream.</param>
        /// <param name="leaveOpen">Whether to leave the stream open when the generated <see cref="StreamReader"/> is disposed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StreamReader GetStreamReader(this Stream stream, Encoding? encoding, bool leaveOpen)
            => GetStreamReader(stream, encoding, DefaultStreamBufferSize, leaveOpen);

        /// <summary>
        /// Returns a <see cref="StreamReader"/> for the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to get the reader for.</param>
        /// <param name="encoding">The encoding to use while reading the stream.</param>
        /// <param name="bufferSize">The buffer size.</param>
        /// <param name="leaveOpen">Whether to leave the stream open when the generated <see cref="StreamReader"/> is disposed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StreamReader GetStreamReader(this Stream stream, Encoding? encoding, int bufferSize, bool leaveOpen)
        {
            Guard.IsNotNull(stream, nameof(stream));

            return new StreamReader(stream, encoding ?? Encoding.UTF8, true, bufferSize, leaveOpen);
        }

        /// <summary>
        /// Returns a <see cref="StreamWriter"/> for the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to get the writer for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StreamWriter GetStreamWriter(this Stream stream)
            => GetStreamWriter(stream, Encoding.UTF8, DefaultStreamBufferSize);

        /// <summary>
        /// Returns a <see cref="StreamWriter"/> for the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to get the writer for.</param>
        /// <param name="encoding">The encoding to use while writing the stream.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StreamWriter GetStreamWriter(this Stream stream, Encoding? encoding)
            => GetStreamWriter(stream, encoding, DefaultStreamBufferSize);

        /// <summary>
        /// Returns a <see cref="StreamWriter"/> for the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to get the writer for.</param>
        /// <param name="bufferSize">The buffer size.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StreamWriter GetStreamWriter(this Stream stream, int bufferSize)
            => GetStreamWriter(stream, Encoding.UTF8, bufferSize);

        /// <summary>
        /// Returns a <see cref="StreamWriter"/> for the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to get the writer for.</param>
        /// <param name="encoding">The encoding to use while writing the stream.</param>
        /// <param name="bufferSize">The buffer size.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StreamWriter GetStreamWriter(this Stream stream, Encoding? encoding, int bufferSize)
        {
            Guard.IsNotNull(stream, nameof(stream));

            return new StreamWriter(stream, encoding ?? Encoding.UTF8, bufferSize, true);
        }

        /// <summary>
        /// Reads all bytes from a stream.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        public static byte[] ReadAllBytes(this Stream stream)
        {
            Guard.IsNotNull(stream, nameof(stream));
            Guard.CanRead(stream, nameof(stream));

            if (!stream.CanSeek)
            {
                // Unfortunately we cannot seek, so we need to copy the bytes over to another array.
                // Not memory efficient, but there is nothing we can do.
                using var msc = new MemoryStream();

                stream.CopyTo(msc);

                return msc.ToArray();
            }

            int index = 0;
            int count = (int)stream.Length;
            byte[] bytes = new byte[count];

            do
            {
                int n = stream.Read(bytes, index, count - index);
                index += n;
            }
            while (index < count);

            return bytes;
        }

        /// <summary>
        /// Reads all bytes from a stream.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="dest">The location to write the bytes.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReadAllBytes(this Stream stream, Span<byte> dest)
        {
            Guard.IsNotNull(stream, nameof(stream));
            Guard.CanRead(stream, nameof(stream));

            stream.Write(dest);
        }

        /// <summary>
        /// Reads all bytes from a stream.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static async ValueTask<byte[]> ReadAllBytesAsync(this Stream stream, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(stream, nameof(stream));
            Guard.CanRead(stream, nameof(stream));

            if (!stream.CanSeek)
            {
                // Unfortunately we cannot seek, so we need to copy the bytes over to another array.
                // Not memory efficient, but there is nothing we can do.
                using var msc = new MemoryStream();

                await stream.CopyToAsync(msc, 81920, cancellationToken).ConfigureAwait(false);

                return msc.ToArray();
            }

            int index = 0;
            int count = (int)stream.Length;
            byte[] bytes = new byte[count];

            do
            {
                int n = await stream.ReadAsync(bytes.AsMemory(index, count - index), cancellationToken).ConfigureAwait(false);
                index += n;
            }
            while (index < count);

            return bytes;
        }

        /// <summary>
        /// Reads the stream to the end.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        public static string ReadToEnd(this Stream stream)
        {
            Guard.IsNotNull(stream, nameof(stream));
            Guard.CanRead(stream, nameof(stream));

            using var streamReader = stream.GetStreamReader();
            return streamReader.ReadToEnd();
        }

        /// <summary>
        /// Reads the stream to the end.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<string> ReadToEndAsync(this Stream stream)
            => ReadToEndAsync(stream, null);

        /// <summary>
        /// Reads the stream to the end.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="encoding">The encoding.</param>
        public static async Task<string> ReadToEndAsync(this Stream stream, Encoding? encoding)
        {
            Guard.IsNotNull(stream, nameof(stream));
            Guard.CanRead(stream, nameof(stream));

            using var streamReader = stream.GetStreamReader(encoding);
            return await streamReader.ReadToEndAsync().ConfigureAwait(false);
        }
    }
}
