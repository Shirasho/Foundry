using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Media.Nintendo64.Rom
{
    public static class RomConverter
    {
        /// <summary>
        /// Converts the content in <paramref name="data"/> to the format
        /// specified in <paramref name="format"/>.
        /// </summary>
        /// <remarks>
        /// This method will make a copy of the data even if the format is not
        /// changed.
        /// </remarks>
        /// <param name="data">The data to convert to.</param>
        /// <param name="format">The format to convert.</param>
        public static IRomData ConvertTo(this IRomData data, ERomFormat format)
        {
            Guard.IsNotNull(data, nameof(data));

            byte[] copy = new byte[data.Length];
            data.GetData().CopyTo(copy);

            ConvertTo(format, copy);

            return RomData.LoadRom(copy);
        }

        /// <summary>
        /// Converts the content in <paramref name="data"/> to the format
        /// specified in <paramref name="format"/>.
        /// </summary>
        /// <remarks>
        /// This method will make a copy of the data even if the format is not
        /// changed.
        /// </remarks>
        /// <param name="data">The data to convert to.</param>
        /// <param name="format">The format to convert.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static async ValueTask<IRomData> ConvertToAsync(this IRomData data, ERomFormat format, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(data, nameof(data));

            byte[] copy = new byte[data.Length];
            var src = await data.GetDataAsync();
            src.CopyTo(copy);

            ConvertTo(format, copy);

            return await RomData.LoadRomAsync(copy, cancellationToken);
        }

        /// <summary>
        /// Makes a copy of <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data to copy.</param>
        public static IRomData Copy(this IRomData data)
            => ConvertTo(data, data.Metadata!.Format);

        /// <summary>
        /// Converts the ROM data in <paramref name="data"/> to the format specified in
        /// <paramref name="format"/>.
        /// </summary>
        /// <param name="format">The format to convert <paramref name="data"/> to.</param>
        /// <param name="data">The data to convert.</param>
        /// <exception cref="ArgumentException"><paramref name="data"/> is not a valid ROM.</exception>
        public static void ConvertTo(ERomFormat format, in Span<byte> data)
        {
            var dataFormat = RomMetadata.GetFormat(data);
            if (dataFormat == ERomFormat.Invalid)
            {
                throw new ArgumentException("Data is not a valid ROM.", nameof(data));
            }

            if (dataFormat == format)
            {
                return;
            }

            switch (format)
            {
                // Switch endians.
                case ERomFormat.BigEndian when dataFormat == ERomFormat.LittleEndian:
                case ERomFormat.LittleEndian when dataFormat == ERomFormat.BigEndian:
                    ByteManipulation.SwapEndian(data);
                    return;
                // Byte-swapped is Big-Endian. If we are already BE based, just swap bytes.
                case ERomFormat.ByteSwapped when dataFormat == ERomFormat.BigEndian:
                case ERomFormat.BigEndian when dataFormat == ERomFormat.ByteSwapped:
                    ByteManipulation.SwapBytes(data);
                    return;
                // Small endian to byte swap. We need to convert to big endian first.
                case ERomFormat.LittleEndian when dataFormat == ERomFormat.ByteSwapped:
                    ByteManipulation.SwapEndian(data);
                    ByteManipulation.SwapBytes(data);
                    return;
                // Byte swap to small endian. We need to convert to big endian first.
                case ERomFormat.ByteSwapped when dataFormat == ERomFormat.LittleEndian:
                    ByteManipulation.SwapBytes(data);
                    ByteManipulation.SwapEndian(data);
                    return;
                default:
#pragma warning disable RCS1140 // Add exception to documentation comment.
                    throw new NotSupportedException($"Unable to convert from {dataFormat} to {format}.");
#pragma warning restore RCS1140 // Add exception to documentation comment.

            }
        }


        /// <summary>
        /// Converts the ROM data in <paramref name="data"/> to the format specified in
        /// <paramref name="format"/>.The result is stored in <paramref name="dest"/>.
        /// </summary>
        /// <param name="format">The format to convert <paramref name="data"/> to.</param>
        /// <param name="data">The data to convert.</param>
        /// <param name="dest">The buffer to store the result in.</param>
        /// <exception cref="ArgumentException"><paramref name="data"/> is not a valid ROM.</exception>
        public static void ConvertTo(ERomFormat format, in ReadOnlySpan<byte> data, in Span<byte> dest)
        {
            var dataFormat = RomMetadata.GetFormat(data);
            if (dataFormat == ERomFormat.Invalid)
            {
                throw new ArgumentException("Data is not a valid ROM.", nameof(data));
            }

            ConvertTo(format, data, dataFormat, dest);
        }

        /// <summary>
        /// Converts the ROM data in <paramref name="data"/> to the format specified in
        /// <paramref name="format"/>.The result is stored in <paramref name="dest"/>.
        /// </summary>
        /// <param name="format">The format to convert <paramref name="data"/> to.</param>
        /// <param name="data">The data to convert.</param>
        /// <param name="dest">The buffer to store the result in.</param>
        /// <exception cref="ArgumentException"><paramref name="data"/> is not a valid ROM.</exception>
        public static void ConvertTo(ERomFormat format, in ReadOnlySpan<byte> data, ERomFormat dataFormat, in Span<byte> dest)
        {
            Guard.HasSizeGreaterThanOrEqualTo(dest, data.Length, nameof(dest));

            if (dataFormat == format)
            {
                data.CopyTo(dest);
                return;
            }

            switch (format)
            {
                // Switch endians.
                case ERomFormat.BigEndian when dataFormat == ERomFormat.LittleEndian:
                case ERomFormat.LittleEndian when dataFormat == ERomFormat.BigEndian:
                    ByteManipulation.SwapEndian(data, dest);
                    return;
                // Byte-swapped is Big-Endian. If we are already BE based, just swap bytes.
                case ERomFormat.ByteSwapped when dataFormat == ERomFormat.BigEndian:
                case ERomFormat.BigEndian when dataFormat == ERomFormat.ByteSwapped:
                    ByteManipulation.SwapBytes(data, dest);
                    return;
                // Small endian to byte swap. We need to convert to big endian first.
                case ERomFormat.LittleEndian when dataFormat == ERomFormat.ByteSwapped:
                    ByteManipulation.SwapEndian(data, dest);
                    ByteManipulation.SwapBytes(dest);
                    return;
                // Byte swap to small endian. We need to convert to big endian first.
                case ERomFormat.ByteSwapped when dataFormat == ERomFormat.LittleEndian:
                    ByteManipulation.SwapBytes(data, dest);
                    ByteManipulation.SwapEndian(dest);
                    return;
                default:
#pragma warning disable RCS1140 // Add exception to documentation comment.
                    throw new NotSupportedException($"Unable to convert from {dataFormat} to {format}.");
#pragma warning restore RCS1140 // Add exception to documentation comment.

            }
        }
    }
}
