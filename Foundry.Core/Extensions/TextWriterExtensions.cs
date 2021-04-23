using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Foundry
{
    public static class TextWriterExtensions
    {
        /// <summary>
        /// Asynchronously writes a string to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="TextWriter">The <see cref="TextWriter"/> instance.</param>
        /// <param name="value">The string to write to the <see cref="TextWriter"/>.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A Task that represents the asynchronous write operation.</returns>
        public static Task WriteAsync(this TextWriter TextWriter, string value, CancellationToken cancellationToken)
        {
            return TextWriter.WriteAsync((value ?? string.Empty).AsMemory(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a string to the <see cref="TextWriter"/>, followed by a line terminator.
        /// </summary>
        /// <param name="TextWriter">The <see cref="TextWriter"/> instance.</param>
        /// <param name="value">The string to write to the <see cref="TextWriter"/>.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A Task that represents the asynchronous write operation.</returns>
        public static Task WriteLineAsync(this TextWriter TextWriter, string value, CancellationToken cancellationToken)
        {
            return TextWriter.WriteLineAsync((value ?? string.Empty).AsMemory(), cancellationToken);
        }
    }
}
