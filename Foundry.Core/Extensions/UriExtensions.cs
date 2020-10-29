using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    public static class UriExtensions
    {
        /// <summary>
        /// Appends the contents of <paramref name="parts"/> to the end of
        /// <paramref name="baseUri"/> and returns a new <see cref="Uri"/>
        /// instance.
        /// </summary>
        /// <param name="baseUri">The base <see cref="Uri"/>.</param>
        /// <param name="parts">The parts to append to the <see cref="Uri"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Uri Append(this Uri baseUri, params string?[]? parts)
            => Append(baseUri, false, parts);

        /// <summary>
        /// Appends the contents of <paramref name="parts"/> to the end of
        /// <paramref name="baseUri"/> and returns a new <see cref="Uri"/>
        /// instance.
        /// </summary>
        /// <param name="baseUri">The base <see cref="Uri"/>.</param>
        /// <param name="isFileSystemUri">Whether the <see cref="Uri"/> points to a file system path.</param>
        /// <param name="parts">The parts to append to the <see cref="Uri"/>.</param>
        public static Uri Append(this Uri baseUri, bool isFileSystemUri, params string?[]? parts)
        {
            Guard.IsNotNull(baseUri, nameof(baseUri));

            char separator = !isFileSystemUri ? '/' : Path.DirectorySeparatorChar;
            parts ??= Array.Empty<string?>();

            if (parts.Length == 0)
            {
                return new Uri(baseUri.ToString());
            }

            if (parts.Length == 1 && !string.IsNullOrWhiteSpace(parts[0]))
            {
                return new Uri(baseUri, parts[0]!.Trim(separator));
            }

            var sb = new StringBuilder();
            foreach (string? part in parts)
            {
                if (string.IsNullOrWhiteSpace(part) || part is null)
                {
                    continue;
                }

                sb.Append(part.Trim(separator)).Append(separator);
            }

            if (sb.Length >= 1)
            {
                sb.RemoveLast();
            }

            return new Uri(baseUri, sb.ToString());
        }
    }
}
