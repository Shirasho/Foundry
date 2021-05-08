using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Toolkit.Diagnostics;
using Foundry.Serialization.Dbase.Level5;
using Foundry.IO;

namespace Foundry.Serialization.Dbase
{
    /// <summary>
    /// A dBASE parser.
    /// </summary>
    public sealed class Parser
    {
        /// <summary>
        /// The parser options.
        /// </summary>
        public ParserOptions ParserOptions { get; }

        /// <inheritdoc />
        public Parser()
            : this(null, null)
        {
        }

        /// <inheritdoc />
        public Parser(CultureInfo? culture)
            : this(null, culture)
        {

        }

        /// <inheritdoc />
        public Parser(Encoding? encoding)
            : this(encoding, null)
        {
        }

        /// <inheritdoc />
        public Parser(Encoding? encoding, CultureInfo? culture)
        {
            var options = new ParserOptions();
            if (encoding is not null)
            {
                options.Encoding = encoding;
            }
            if (culture is not null)
            {
                options.Culture = culture;
            }

            ParserOptions = options;
        }

        public Parser(ParserOptions? options)
        {
            ParserOptions = options ?? new ParserOptions();
        }

        /// <summary>
        /// Converts a DBF file into a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="filePath">The path to the DBF file.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is <see langword="null"/></exception>
        /// <exception cref="UnauthorizedAccessException">Access to <paramref name="filePath"/> is denied.</exception>
        /// <exception cref="ArgumentException">The file name is empty, contains only white spaces, or contains invalid characters.</exception>
        /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
        /// <exception cref="NotSupportedException"><paramref name="filePath"/> contains a colon (:) in the middle of the string.</exception>
        /// <exception cref="IOException">The file is already open.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        public DataTable Parse(string filePath, in CancellationToken cancellationToken = default)
            => Parse(filePath, false, cancellationToken);

        /// <summary>
        /// Converts a DBF file into a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="filePath">The path to the DBF file.</param>
        /// <param name="throwOnMissingFile">Whether to throw an exception if the file does not exist.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is <see langword="null"/></exception>
        /// <exception cref="UnauthorizedAccessException">Access to <paramref name="filePath"/> is denied.</exception>
        /// <exception cref="ArgumentException">The file name is empty, contains only white spaces, or contains invalid characters.</exception>
        /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
        /// <exception cref="NotSupportedException"><paramref name="filePath"/> contains a colon (:) in the middle of the string.</exception>
        /// <exception cref="IOException">The file is already open.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        public DataTable Parse(string filePath, bool throwOnMissingFile, in CancellationToken cancellationToken = default)
        {
            Guard.IsNotNullOrWhiteSpace(filePath, nameof(filePath));

            return Parse(new FileInfo(filePath), throwOnMissingFile, cancellationToken);
        }

        /// <summary>
        /// Converts a DBF file into a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="file">The path to the DBF file.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> is <see langword="null"/></exception>
        /// <exception cref="UnauthorizedAccessException"><paramref name="file"/> is read-only or is a directory.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="IOException">The file is already open.</exception>
        public DataTable Parse(FileInfo file, in CancellationToken cancellationToken = default)
            => Parse(file, false, cancellationToken);

        /// <summary>
        /// Converts a DBF file into a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="file">The path to the DBF file.</param>
        /// <param name="throwOnMissingFile">Whether to throw an exception if the file does not exist.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> is <see langword="null"/></exception>
        /// <exception cref="UnauthorizedAccessException"><paramref name="file"/> is read-only or is a directory.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="IOException">The file is already open.</exception>
        public DataTable Parse(FileInfo file, bool throwOnMissingFile, in CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(file, nameof(file));

            cancellationToken.ThrowIfCancellationRequested();

            file.Refresh();
            if (!file.Exists)
            {
                return throwOnMissingFile
                    ? throw new IOException($"The file {file.FullName} does not exist.")
                    : new DataTable();
            }

            using var fs = file.OpenRead();
            return GetDataTable(fs, cancellationToken);
        }

        /// <summary>
        /// Converts the contents of a stream into a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="stream">The DBF data stream.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/></exception>
        public DataTable Parse(Stream stream, in CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(stream, nameof(stream));

            return GetDataTable(stream, cancellationToken);
        }

        private DataTable GetDataTable(Stream stream, in CancellationToken cancellationToken = default)
        {
            var dataTable = new DataTable();

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var br = new BufferedBinaryReader(stream);

                int versionInfo = br.Peek();
                var level = (EDatabaseLevel)(versionInfo & 0b111);

                var parser = level switch
                {
                    EDatabaseLevel.Five => new LevelFiveParser(ParserOptions),
                    //http://www.dbase.com/KnowledgeBase/int/db7_file_fmt.htm
                    //EDatabaseLevel.Seven => new LevelSevenParser(Encoding, Culture),
                    _ => throw new NotSupportedException($"The dBASE level {Convert.ToString(versionInfo & 0b111, 2)} [{versionInfo & 0b111}] is not supported.")
                };

                parser.Parse(ref br, dataTable, cancellationToken);

                return dataTable;
            }
            catch
            {
                dataTable.Clear();
                dataTable.Dispose();
                throw;
            }
        }
    }
}
