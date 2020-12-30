using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Foundary.Serialization.Dbase;
using Foundry.IO;

namespace Foundry.Serialization.Dbase.Level5
{
    /// <summary>
    /// Level 7 database field descriptors.
    /// </summary>
    [DebuggerDisplay("Field: {" + nameof(FieldName) + "}, Type: {" + nameof(FieldType) + "}, Length: {" + nameof(FieldLength) + "}")]
    public class DatabaseFieldDescriptor
    {
        /// <summary>
        /// The field name.
        /// </summary>
        /// <remarks>Bytes 0-10 (11)</remarks>
        public readonly byte[] FieldName;

        /// <summary>
        /// The data type of the field.
        /// </summary>
        /// <remarks>Byte 11</remarks>
        public readonly EFieldType FieldType;

        /// <remarks>Bytes 12-15 (4)</remarks>
        [SuppressMessage("Code Quality", "IDE0052:Remove unread private members", Justification = "<Pending>")]
        private readonly byte[] Reserved01;

        /// <summary>
        /// The field length in bytes.
        /// </summary>
        /// <remarks>Byte 16</remarks>
        public readonly byte FieldLength;

        /// <summary>
        /// The field precision.
        /// </summary>
        /// <remarks>Byte 17</remarks>
        public readonly byte FieldPrecision;

        /// <summary>
        /// Field flags (potentially reserved, conflicting sources)
        /// </summary>
        /// <remarks>Byte 18</remarks>
        [Obsolete("Dangerous to use, as there are conflicting sources saying this field exists.")]
        public readonly EFieldFlags FieldFlags;

        /// <remarks>Bytes 19</remarks>
        [SuppressMessage("Code Quality", "IDE0052:Remove unread private members", Justification = "<Pending>")]
        private readonly byte Reserved02;

        /// <summary>
        /// The work area Id.
        /// </summary>
        /// <remarks>Byte 20</remarks>
        public readonly byte WorkAreaId;

        /// <remarks>Bytes 21-30 (10)</remarks>
        [SuppressMessage("Code Quality", "IDE0052:Remove unread private members", Justification = "<Pending>")]
        private readonly byte[] Reserved03;

        /// <summary>
        /// Defines what flags exist for this table.
        /// </summary>
        /// <remarks>Byte 37</remarks>
        public readonly ETableFlags TableFlags;

        private readonly Encoding Encoding;
        private readonly CultureInfo NumericCulture;
        private readonly CultureInfo DateCulture;

        public DatabaseFieldDescriptor(ref BufferedBinaryReader reader, ParserOptions parserOptions)
        {
            Encoding = parserOptions.Encoding ?? Encoding.ASCII;
            NumericCulture = parserOptions.InvariantNumbers ? CultureInfo.InvariantCulture : parserOptions.Culture ?? CultureInfo.InvariantCulture;
            DateCulture = parserOptions.InvariantDates ? CultureInfo.InvariantCulture : parserOptions.Culture ?? CultureInfo.InvariantCulture;

            FieldName = reader.ReadBytes(11);
            FieldType = (EFieldType)reader.ReadByte();
            Reserved01 = reader.ReadBytes(4);
            FieldLength = reader.ReadByte();
            FieldPrecision = reader.ReadByte();
#pragma warning disable 618
            FieldFlags = (EFieldFlags)reader.ReadByte();
#pragma warning restore 618
            Reserved02 = reader.ReadByte();
            WorkAreaId = reader.ReadByte();
            Reserved03 = reader.ReadBytes(10);
            TableFlags = (ETableFlags)reader.ReadByte();
        }

        /// <summary>
        /// Obtains the value of this field from the specified <see cref="BufferedBinaryReader"/>.
        /// </summary>
        /// <param name="br">The <see cref="BufferedBinaryReader"/> to read from.</param>
        /// <returns>The object that was read.</returns>
        /// <remarks>
        /// While it may technically be possible to remove this boxing by doing the switch statement
        /// outside of the class (where this method is being used), the result goes directly into
        /// a DataRow which boxes anyway.
        /// </remarks>
        internal object GetValue(ref BufferedBinaryReader br)
        {
            //IMPLEMENT: Support for DBT files. https://en.wikipedia.org/wiki/.dbf#Level_5_DOS_headers
            try
            {
                switch (FieldType)
                {
                    case EFieldType.Memo:
                    case EFieldType.Character:
                        //TODO: Can we do this without allocation of another string that trims the 0-bytes? Is it safe to remove trailing 0 bytes?
                        return Encoding.GetString(br.ReadBytes(FieldLength)).Trim('\0');
                    case EFieldType.Binary:
                    case EFieldType.General:
                        // We do not support DBT tables.
                        return DBNull.Value;
                    case EFieldType.Float:
                    case EFieldType.Numeric:
                        // We cannot simply assume it takes up 4 or 8 bytes for a float or double respectively.
                        return double.Parse(Encoding.GetString(br.ReadBytes(FieldLength)), NumericCulture);
                    case EFieldType.Logical:
                    {
                        string value = Encoding.GetString(br.ReadBytes(FieldLength));

                        return value == "Y" || value == "y"
                            ? true
                            : value == "?"
                                ? (object)DBNull.Value
                                : false;
                    }
                    case EFieldType.Date: return DateTime.TryParseExact(Encoding.GetString(br.ReadBytes(FieldLength)), "yyyyMMdd", DateCulture, DateTimeStyles.None, out var result) ? result : (object)DBNull.Value;
                    case EFieldType.DateTime: return DateTime.FromOADate(br.ReadInt64() - 2415018.5);
                    default: return DBNull.Value;
                }
            }
            catch (FormatException e)
            {
                throw new FormatException($"Input string was not in a correct format [DbType = {FieldType}].", e);
            }
        }

        /// <summary>
        /// Returns the <see cref="DataColumn"/> type for this field.
        /// </summary>
        internal Type? GetValueType()
        {
            return FieldType switch
            {
                EFieldType.Float => typeof(double),
                EFieldType.Numeric => typeof(double),
                EFieldType.General => typeof(byte[]),
                EFieldType.Binary => typeof(byte[]),
                EFieldType.Character => typeof(string),
                EFieldType.Memo => typeof(string),
                EFieldType.Date => typeof(DateTime),
                EFieldType.DateTime => typeof(DateTime),
                EFieldType.Logical => typeof(bool),
                _ => null
            };
        }
    }
}
