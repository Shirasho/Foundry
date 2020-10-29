using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using Foundry.Linq;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    public static class CompressionExtensions
    {
        private static readonly string[] DefaultPattern = new[] { "*.*" };

        /// <summary>
        /// Compresses the contents of <paramref name="file"/> into the archive.
        /// </summary>
        /// <param name="archive">The archive.</param>
        /// <param name="file">The file to compress.</param>
        /// <param name="destination">The destination in the archive.</param>
        /// <param name="compressionLevel">The compression level.</param>
        public static void CompressFile(this ZipArchive archive, string file, string? destination, CompressionLevel compressionLevel)
        {
            Guard.IsNotNull(archive, nameof(archive));
            Guard.IsNotNullOrWhitespace(file, nameof(file));
            GuardEx.IsValid(compressionLevel, nameof(compressionLevel));

            string desiredFileName = !string.IsNullOrWhiteSpace(destination) ? destination! : Path.GetFileName(file);
            try
            {
                archive.CreateEntryFromFile(file, desiredFileName, compressionLevel);
            }
            catch (IOException e)
            {
                string tempFile = Path.GetTempFileName();
                try
                {
                    // The file is locked. Try copying to the temp directory, copy from there,
                    // then delete the copy.
                    File.Copy(file, tempFile, true);
                    archive.CreateEntryFromFile(tempFile, desiredFileName, compressionLevel);
                }
                catch (Exception)
                {
                    e.RestoreAndThrow();
                }
                finally
                {
                    try
                    {
                        File.Delete(tempFile);
                    }
                    catch
                    {
                        // Suppress
                    }
                }
            }
        }

        /// <summary>
        /// Compresses the contents of <paramref name="file"/> into the archive.
        /// </summary>
        /// <param name="archive">The archive.</param>
        /// <param name="file">The file to compress.</param>
        /// <param name="destination">The destination in the archive.</param>
        /// <param name="compressionLevel">The compression level.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CompressFile(this ZipArchive archive, FileInfo file, string? destination, CompressionLevel compressionLevel)
        {
            Guard.IsNotNull(file, nameof(file));

            CompressFile(archive, file.FullName, destination, compressionLevel);
        }

        /// <summary>
        /// Compresses the contents of <paramref name="directory"/> that match <paramref name="patterns"/> into the archive.
        /// </summary>
        /// <param name="archive">The archive.</param>
        /// <param name="directory">The directory to search in.</param>
        /// <param name="destination">The path in the archive to save the directory to.</param>
        /// <param name="patterns">The file patterns.</param>
        /// <param name="ignoredFileExtensions">The extensions of files to not compress.</param>
        /// <param name="compressionLevel">The compression level.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CompressDirectory(this ZipArchive archive, DirectoryInfo directory,
                                             string? destination = null, IReadOnlyCollection<string>? patterns = null,
                                             IReadOnlyCollection<string>? ignoredFileExtensions = null,
                                             CompressionLevel compressionLevel = CompressionLevel.Optimal)
            => CompressDirectory(archive, directory, directory, destination, patterns, ignoredFileExtensions, compressionLevel);

        private static void CompressDirectory(ZipArchive archive, DirectoryInfo directory, DirectoryInfo? currentDirectory,
                                              string? destination = null, IReadOnlyCollection<string>? patterns = null,
                                              IReadOnlyCollection<string>? ignoredFileExtensions = null,
                                              CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            currentDirectory ??= directory;
            patterns ??= DefaultPattern;

            foreach (var subdirectory in currentDirectory.GetDirectories())
            {
                CompressDirectory(archive, directory, subdirectory, destination, patterns, ignoredFileExtensions, compressionLevel);
            }

            var ignored = ignoredFileExtensions ?? Array.Empty<string>();
            patterns = patterns?.Count > 0 && patterns.Any(p => !string.IsNullOrWhiteSpace(p))
                ? patterns
                : new[] { "*.*" };

            foreach (string pattern in patterns)
            {
                foreach (var file in currentDirectory.GetFiles(pattern))
                {
                    if (ignored.Contains(file.Extension, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string desiredFileName = file.FullName.Replace(directory.FullName, string.Empty);
                    if (!string.IsNullOrWhiteSpace(destination))
                    {
                        desiredFileName = string.Concat(destination.EnsureEndsWith(Path.DirectorySeparatorChar), desiredFileName.TrimStart(Path.DirectorySeparatorChar));
                    }
                    archive.CompressFile(file, desiredFileName, compressionLevel);
                }
            }
        }
    }
}
