using System;
using System.IO;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    public static partial class FileSystemInfoExtensions
    {
        /// <summary>
        /// Creates a new <see cref="DirectoryInfo"/> using the specified directory as the base path, then
        /// appends the <paramref name="subdirectoryName"/> subdirectory onto the end.
        /// </summary>
        /// <param name="directory">The directory to append to.</param>
        /// <param name="subdirectoryName">The subdirectory name to append.</param>
        /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException"><paramref name="subdirectoryName"/> is <see langword="null"/> or empty.</exception>
        public static DirectoryInfo CombineDirectory(this DirectoryInfo directory, string subdirectoryName)
        {
            Guard.IsNotNull(directory, nameof(directory));
            Guard.IsNotNullOrEmpty(subdirectoryName, nameof(subdirectoryName));

            return new DirectoryInfo(Path.Combine(directory.FullName, subdirectoryName));
        }

        /// <summary>
        /// Creates a new <see cref="DirectoryInfo"/> using the specified directory as the base path, then
        /// appends <paramref name="fragments"/> onto the end.
        /// </summary>
        /// <param name="directory">The directory to append to.</param>
        /// <param name="fragments">The fragments to append.</param>
        /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <see langword="null"/></exception>
        public static DirectoryInfo CombineDirectory(this DirectoryInfo directory, params string[]? fragments)
        {
            Guard.IsNotNull(directory, nameof(directory));

            if (fragments is null || fragments.Length == 0)
            {
                return new DirectoryInfo(directory.FullName);
            }

            return new DirectoryInfo(directory.FullName.EnsureEndsWith(Path.DirectorySeparatorChar) + Path.Combine(fragments));
        }

        /// <summary>
        /// Creates a new <see cref="FileInfo"/> using the specified directory as the base path, then
        /// appends the <paramref name="fileName"/> onto the end.
        /// </summary>
        /// <param name="directory">The directory to append to.</param>
        /// <param name="fileName">The filename to append.</param>
        /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException"><paramref name="fileName"/> is <see langword="null"/>, empty, or whitespace.</exception>
        public static FileInfo CombineFile(this DirectoryInfo directory, string fileName)
        {
            Guard.IsNotNull(directory, nameof(directory));
            Guard.IsNotNullOrWhitespace(fileName, nameof(fileName));

            return new FileInfo(Path.Combine(directory.FullName, fileName));
        }

        /// <summary>
        /// Creates a new <see cref="FileInfo"/> using the specified directory as the base path, then
        /// appends <paramref name="fragments"/> onto the end.
        /// </summary>
        /// <param name="directory">The directory to append to.</param>
        /// <param name="fragments">The fragments to append.</param>
        /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <see langword="null"/></exception>
        public static FileInfo CombineFile(this DirectoryInfo directory, params string[]? fragments)
        {
            Guard.IsNotNull(directory, nameof(directory));

            if (fragments is null || fragments.Length == 0)
            {
                return new FileInfo(directory.FullName);
            }

            return new FileInfo(directory.FullName.EnsureEndsWith(Path.DirectorySeparatorChar) + Path.Combine(fragments));
        }
    }
}
