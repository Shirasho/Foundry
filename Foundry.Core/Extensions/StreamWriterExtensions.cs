using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Foundry
{
    public static class StreamWriterExtensions
    {
        /// <summary>
        /// Asynchronously writes a string to the stream.
        /// </summary>
        /// <param name="streamWriter">The <see cref="StreamWriter"/> instance.</param>
        /// <param name="value">The string to write to the stream.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A Task that represents the asynchronous write operation.</returns>
        public static Task WriteAsync(this StreamWriter streamWriter, string value, CancellationToken cancellationToken)
        {
            return streamWriter.WriteAsync((value ?? string.Empty).AsMemory(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a string to the stream, followed by a line terminator.
        /// </summary>
        /// <param name="streamWriter">The <see cref="StreamWriter"/> instance.</param>
        /// <param name="value">The string to write to the stream.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A Task that represents the asynchronous write operation.</returns>
        public static Task WriteLineAsync(this StreamWriter streamWriter, string value, CancellationToken cancellationToken)
        {
            return streamWriter.WriteLineAsync((value ?? string.Empty).AsMemory(), cancellationToken);
        }
    }
}
