using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Foundry.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    public static partial class FileSystemInfoExtensions
    {
        /// <summary>
        /// Clears the contents of a file.
        /// </summary>
        /// <param name="fileInfo">The file to clear.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo"/> is <see langword="null"/></exception>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="UnauthorizedAccessException">Access is denied.</exception>
        public static void Clear(this FileInfo fileInfo)
        {
            Guard.IsNotNull(fileInfo, nameof(fileInfo));

            Overwrite(fileInfo, (byte[]?)null);

            fileInfo.Refresh();
            if (!fileInfo.Exists)
            {
                return;
            }

            try
            {
                using var sw = new StreamWriter(fileInfo.FullName, false, Encoding.UTF8);
                sw.Write(string.Empty);
                sw.Flush();
            }
            catch (DirectoryNotFoundException)
            {
                // Not possible.
            }
        }

        /// <summary>
        /// Creates a backup of <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The file to back up.</param>
        /// <returns>A <see cref="FileInfo"/> instance representing the backup file.</returns>
        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FileInfo CreateBackup(this FileInfo file)
            => CreateBackup(file, true);

        /// <summary>
        /// Creates a backup of <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The file to back up.</param>
        /// <param name="sameDirectory">Whether to create the backup in the same directory as the original file. If <see langword="false"/>,
        /// the file will be created in a subdirectory called "Backups".</param>
        /// <returns>A <see cref="FileInfo"/> instance representing the backup file.</returns>
        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FileInfo CreateBackup(this FileInfo file, bool sameDirectory)
            => CreateBackup(file, sameDirectory, null);

        /// <summary>
        /// Creates a backup of <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The file to back up.</param>
        /// <param name="backupDirectory">The directory to place the backup file.</param>
        /// <returns>A <see cref="FileInfo"/> instance representing the backup file.</returns>
        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FileInfo CreateBackup(this FileInfo file, DirectoryInfo backupDirectory)
            => CreateBackup(file, backupDirectory, null);

        /// <summary>
        /// Creates a backup of <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The file to back up.</param>
        /// <param name="backupFileName">The name to give the backup file.</param>
        /// <returns>A <see cref="FileInfo"/> instance representing the backup file.</returns>
        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FileInfo CreateBackup(this FileInfo file, string backupFileName)
            => CreateBackup(file, true, backupFileName);

        /// <summary>
        /// Creates a backup of <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The file to back up.</param>
        /// <param name="backupDirectoryName">The name of the backup subdirectory.</param>
        /// <param name="backupFileName">The name to give the backup file.</param>
        /// <returns>A <see cref="FileInfo"/> instance representing the backup file.</returns>
        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FileInfo CreateBackup(this FileInfo file, string? backupDirectoryName, string? backupFileName)
        {
            Guard.IsNotNull(file, nameof(file));

            var backupDir = !string.IsNullOrWhiteSpace(backupDirectoryName)
                ? file.Directory?.CombineDirectory(backupDirectoryName!)
                : file.Directory;
            return CreateBackup(file, backupDir, backupFileName);
        }

        /// <summary>
        /// Creates a backup of <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The file to back up.</param>
        /// <param name="sameDirectory">Whether to create the backup in the same directory as the original file. If <see langword="false"/>,
        /// the file will be created in a subdirectory called "Backups".</param>
        /// <param name="backupFileName">The name to give the backup file.</param>
        /// <returns>A <see cref="FileInfo"/> instance representing the backup file.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FileInfo CreateBackup(this FileInfo file, bool sameDirectory, string? backupFileName)
        {
            Guard.IsNotNull(file, nameof(file));

            var backupDir = !sameDirectory ? file.Directory?.CombineDirectory("Backup") : file.Directory;
            return CreateBackup(file, backupDir, backupFileName);
        }

        /// <summary>
        /// Creates a backup of <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The file to back up.</param>
        /// <param name="backupDirectory">The directory to save the backup file to.</param>
        /// <param name="backupFileName">The name to give the backup file.</param>
        /// <returns>A <see cref="FileInfo"/> instance representing the backup file.</returns>
        /// <exception cref="InvalidOperationException">The backup directory cannot be determined.</exception>
        public static FileInfo CreateBackup(this FileInfo file, DirectoryInfo? backupDirectory, string? backupFileName)
        {
            Guard.IsNotNull(file, nameof(file));

            string fileName = !string.IsNullOrWhiteSpace(backupFileName)
                ? backupFileName!
                : $"{DateTimeOffset.Now.ToFileNameTime()}_{Path.GetFileNameWithoutExtension(file.Name)}".TrimEnd('_');
            var backupDir = backupDirectory ?? file.Directory;
            if (backupDir is null)
            {
                throw new InvalidOperationException("Cannot determine backup directory.");
            }
            backupDir.Create();

            var backupFile = backupDir.CombineFile($"{fileName}{file.Extension}");
            file.CopyTo(backupFile.FullName, true);

            return backupFile;
        }

        /// <summary>
        /// Creates a file if it does not exist. Any missing directories will be created recursively.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo"/> is <see langword="null"/></exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="IOException">The directory cannot be created.</exception>
        /// <exception cref="UnauthorizedAccessException"><paramref name="fileInfo"/> is read-only or is a directory.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FileStream CreateSafeStream(this FileInfo fileInfo)
            => CreateSafeStream(fileInfo, FileMode.OpenOrCreate);

        /// <summary>
        /// Creates a file if it does not exist. Any missing directories will be created recursively.
        /// </summary>
        /// <param name="fileInfo">The file to open.</param>
        /// <param name="fileMode">A <see cref="FileMode"/> constant specifying the mode (for example, Open or Append) in which to open the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo"/> is <see langword="null"/></exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="IOException">The directory cannot be created.</exception>
        /// <exception cref="UnauthorizedAccessException"><paramref name="fileInfo"/> is read-only or is a directory.</exception>
        public static FileStream CreateSafeStream(this FileInfo fileInfo, FileMode fileMode)
        {
            Guard.IsNotNull(fileInfo, nameof(fileInfo));

            fileInfo.Refresh();
            if (!fileInfo.Exists)
            {
                fileInfo.Directory?.Create();
                return fileInfo.Create();
            }

            return fileInfo.Open(fileMode, FileAccess.ReadWrite);
        }

        /// <summary>
        /// Overwrites the contents of a file. If the file does not exist, the file is created.
        /// </summary>
        /// <param name="fileInfo">The file to clear.</param>
        /// <param name="content">The content to write.</param>
        /// <param name="encoding">The encoding to use to obtain the content bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo"/> is <see langword="null"/></exception>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="UnauthorizedAccessException">Access is denied.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Overwrite(this FileInfo fileInfo, string? content, Encoding? encoding = null)
        {
            if (content is null)
            {
                Overwrite(fileInfo, Array.Empty<byte>());
                return;
            }

            Overwrite(fileInfo, (encoding ?? Encoding.UTF8).GetBytes(content));
        }

        /// <summary>
        /// Overwrites the contents of a file.
        /// </summary>
        /// <param name="fileInfo">The file to clear.</param>
        /// <param name="content">The content to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo"/> is <see langword="null"/></exception>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="UnauthorizedAccessException">Access is denied.</exception>
        public static void Overwrite(this FileInfo fileInfo, byte[]? content)
        {
            Guard.IsNotNull(fileInfo, nameof(fileInfo));

            using var fs = fileInfo.CreateSafeStream(FileMode.Create);
            byte[] buffer = content ?? Array.Empty<byte>();
            fs.Write(buffer, 0, buffer.Length);
            fs.Flush();
        }

        /// <summary>
        /// Overwrites the contents of a file.
        /// </summary>
        /// <param name="fileInfo">The file to clear.</param>
        /// <param name="content">The content to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo"/> is <see langword="null"/></exception>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="UnauthorizedAccessException">Access is denied.</exception>
        public static void Overwrite(this FileInfo fileInfo, ReadOnlySpan<byte> content)
        {
            Guard.IsNotNull(fileInfo, nameof(fileInfo));

            using var fs = fileInfo.CreateSafeStream(FileMode.Create);
            fs.Write(content);
            fs.Flush();
        }

        /// <summary>
        /// Overwrites the contents of a file. If the file does not exist, the file is created.
        /// </summary>
        /// <param name="fileInfo">The file to clear.</param>
        /// <param name="content">The content to write.</param>
        /// <param name="encoding">The encoding.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo"/> is <see langword="null"/> -or- <paramref name="encoding"/> is <see langword="null"/></exception>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="UnauthorizedAccessException">Access is denied.</exception>
        public static void Overwrite(this FileInfo fileInfo, ReadOnlySpan<char> content, Encoding encoding)
        {
            Guard.IsNotNull(encoding, nameof(encoding));

            int byteCount = encoding.GetByteCount(content);

            using var buffer = MemoryPool<byte>.Shared.Rent(byteCount);
            encoding.GetBytes(content, buffer.Memory.Span);
            Overwrite(fileInfo, buffer.Memory.Span);
        }

        /// <summary>
        /// Overwrites the contents of a file.
        /// </summary>
        /// <param name="fileInfo">The file to clear.</param>
        /// <param name="content">The content to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo"/> is <see langword="null"/></exception>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="UnauthorizedAccessException">Access is denied.</exception>
        public static void Overwrite(this FileInfo fileInfo, Stream content)
        {
            Guard.IsNotNull(fileInfo, nameof(fileInfo));
            Guard.IsNotNull(content, nameof(content));

            using var fs = fileInfo.CreateSafeStream(FileMode.Create);
            content.CopyTo(fs);
            fs.Flush();
        }

        /// <summary>
        /// Overwrites the contents of a file. If the file does not exist, the file is created.
        /// </summary>
        /// <param name="fileInfo">The file to clear.</param>
        /// <param name="content">The content to write.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo"/> is <see langword="null"/></exception>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="UnauthorizedAccessException">Access is denied.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task OverwriteAsync(this FileInfo fileInfo, string? content, CancellationToken cancellationToken = default)
            => OverwriteAsync(fileInfo, content, null, cancellationToken);

        /// <summary>
        /// Overwrites the contents of a file. If the file does not exist, the file is created.
        /// </summary>
        /// <param name="fileInfo">The file to clear.</param>
        /// <param name="content">The content to write.</param>
        /// <param name="encoding">The encoding to use to obtain the content bytes.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo"/> is <see langword="null"/></exception>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="UnauthorizedAccessException">Access is denied.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task OverwriteAsync(this FileInfo fileInfo, string? content, Encoding? encoding = null, CancellationToken cancellationToken = default)
        {
            if (content is null)
            {
                return OverwriteAsync(fileInfo, Array.Empty<byte>(), cancellationToken);
            }

            return OverwriteAsync(fileInfo, (encoding ?? Encoding.UTF8).GetBytes(content), cancellationToken);
        }

        /// <summary>
        /// Overwrites the contents of a file. If the file does not exist, the file is created.
        /// </summary>
        /// <param name="fileInfo">The file to clear.</param>
        /// <param name="content">The content to write.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo"/> is <see langword="null"/></exception>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="UnauthorizedAccessException">Access is denied.</exception>
        public static async Task OverwriteAsync(this FileInfo fileInfo, byte[]? content, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(fileInfo, nameof(fileInfo));

            await using var fs = fileInfo.CreateSafeStream(FileMode.Create);
            byte[] buffer = content ?? Array.Empty<byte>();
            await fs.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
            await fs.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Overwrites the contents of a file. If the file does not exist, the file is created.
        /// </summary>
        /// <param name="fileInfo">The file to clear.</param>
        /// <param name="content">The content to write.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo"/> is <see langword="null"/></exception>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="UnauthorizedAccessException">Access is denied.</exception>
        public static async Task OverwriteAsync(this FileInfo fileInfo, ReadOnlyMemory<byte> content, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(fileInfo, nameof(fileInfo));

            await using var fs = fileInfo.CreateSafeStream(FileMode.Create);
            await fs.WriteAsync(content, cancellationToken).ConfigureAwait(false);
            await fs.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads all text from a file.
        /// </summary>
        /// <param name="fileInfo">The file to read from.</param>
        /// <returns>The contents of the file.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadAllText(this FileInfo fileInfo)
            => ReadAllText(fileInfo, null);

        /// <summary>
        /// Reads all text from a file.
        /// </summary>
        /// <param name="fileInfo">The file to read from.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <returns>The contents of the file.</returns>
        public static string ReadAllText(this FileInfo fileInfo, Encoding? encoding)
        {
            Guard.IsNotNull(fileInfo, nameof(fileInfo));
            encoding ??= Encoding.UTF8;

            using var fs = fileInfo.OpenRead();
            using var sr = fs.GetStreamReader(encoding);
            return sr.ReadToEnd();
        }

        /// <summary>
        /// Reads all text from a file.
        /// </summary>
        /// <param name="fileInfo">The file to read from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The contents of the file.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<string> ReadAllTextAsync(this FileInfo fileInfo, CancellationToken cancellationToken)
            => ReadAllTextAsync(fileInfo, null, cancellationToken);

        /// <summary>
        /// Reads all text from a file.
        /// </summary>
        /// <param name="fileInfo">The file to read from.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The contents of the file.</returns>
        public static async Task<string> ReadAllTextAsync(this FileInfo fileInfo, Encoding? encoding, CancellationToken cancellationToken)
        {
            Guard.IsNotNull(fileInfo, nameof(fileInfo));
            encoding ??= Encoding.UTF8;

            await using var fs = fileInfo.OpenRead();
            using var sr = fs.GetStreamReader(encoding);
            return await sr.ReadToEndAsync().WithCancellation(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads all lines from a file.
        /// </summary>
        /// <param name="fileInfo">The file to read from.</param>
        /// <returns>The lines of the file.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<string> ReadAllLines(this FileInfo fileInfo)
            => ReadAllLines(fileInfo, null);

        /// <summary>
        /// Reads all text from a file.
        /// </summary>
        /// <param name="fileInfo">The file to read from.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <returns>The lines of the file.</returns>
        public static IEnumerable<string> ReadAllLines(this FileInfo fileInfo, Encoding? encoding)
        {
            Guard.IsNotNull(fileInfo, nameof(fileInfo));

            return _(); IEnumerable<string> _()
            {
                encoding ??= Encoding.UTF8;

                using var fs = fileInfo.OpenRead();
                using var sr = fs.GetStreamReader(encoding);

                foreach (string line in sr.ReadAllLines())
                {
                    yield return line;
                }
            }
        }

        /// <summary>
        /// Reads all lines from a file.
        /// </summary>
        /// <param name="fileInfo">The file to read from.</param>
        /// <returns>The lines of the file.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IAsyncEnumerable<string> ReadAllLinesAsync(this FileInfo fileInfo)
            => ReadAllLinesAsync(fileInfo, null);

        /// <summary>
        /// Reads all text from a file.
        /// </summary>
        /// <param name="fileInfo">The file to read from.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <returns>The lines of the file.</returns>
        public static IAsyncEnumerable<string> ReadAllLinesAsync(this FileInfo fileInfo, Encoding? encoding)
        {
            Guard.IsNotNull(fileInfo, nameof(fileInfo));

            return _(); async IAsyncEnumerable<string> _()
            {
                encoding ??= Encoding.UTF8;

                await using var fs = fileInfo.OpenRead();
                using var sr = fs.GetStreamReader(encoding);
                await foreach (string line in sr.ReadAllLinesAsync())
                {
                    yield return line;
                }
            }
        }

        public static FileInfo WithExtension(this FileInfo fileInfo, string extension)
        {
            Guard.IsNotNull(fileInfo, nameof(fileInfo));

            string directoryName = fileInfo.Directory?.FullName ?? string.Empty;
            return new FileInfo(Path.Combine(directoryName, Path.GetFileNameWithoutExtension(fileInfo.Name) + extension.EnsureStartsWith('.')));
        }
    }
}
