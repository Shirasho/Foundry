using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Foundry.IO;

namespace Foundry.Serialization.Dbase.Level5
{
    /// <summary>
    /// Level 7 database header information.
    /// </summary>
    /// <remarks>http://www.dbase.com/KnowledgeBase/int/db7_file_fmt.htm</remarks>
    [DebuggerDisplay("Version: {" + nameof(Version) + "}, RecordCount: {" + nameof(RecordCount) + "}, TableFlags: {" + nameof(TableFlags) + "}")]
    [SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Required for bit correctness.")]
    public readonly ref struct DatabaseHeader
    {
        /// <summary>
        /// The level of the database.
        /// </summary>
        /// <remarks>Byte 0, Bits 0-2</remarks>
        public readonly EDatabaseLevel Level;

        /// <summary>
        /// The version.
        /// </summary>
        /// <remarks>Byte 0, Bits 3-7</remarks>
        public readonly EDatabaseVersion Version;

        /// <summary>
        /// The update year.
        /// </summary>
        /// <remarks>Byte 1</remarks>
        public readonly byte UpdateYear;

        /// <summary>
        /// The update month.
        /// </summary>
        /// <remarks>Byte 2</remarks>
        public readonly byte UpdateMonth;

        /// <summary>
        /// The update date.
        /// </summary>
        /// <remarks>Byte 3</remarks>
        public readonly byte UpdateDay;

        /// <summary>
        /// The number of records.
        /// </summary>
        /// <remarks>Bytes 4-7</remarks>
        public readonly int RecordCount;

        /// <summary>
        /// The length of the header in bytes.
        /// </summary>
        /// <remarks>Bytes 8-9</remarks>
        public readonly short HeaderLength;

        /// <summary>
        /// The length of the records in bytes.
        /// </summary>
        /// <remarks>Bytes 10-11</remarks>
        public readonly short RecordLength;

        /// <remarks>Bytes 12-13 (2)</remarks>
        private readonly byte[] Reserved01;

        /// <summary>
        /// A flag indicating an incomplete dBASE IV transaction.
        /// </summary>
        /// <remarks>Byte 14</remarks>
        public readonly byte IncompleteTransaction;

        /// <summary>
        /// A flag indicating dBASE IV encryption.
        /// </summary>
        /// <remarks>Byte 15</remarks>
        public readonly byte EncryptionFlag;

        /// <summary>
        /// Reserved for multi-user processing.
        /// </summary>
        /// <remarks>Bytes 16-27 (12)</remarks>
        private readonly byte[] Reserved02;

        /// <summary>
        /// Previously 'MDX'. Defines what flags exist for this table.
        /// </summary>
        /// <remarks>Byte 28</remarks>
        public readonly ETableFlags TableFlags;

        /// <summary>
        /// The language driver Id.
        /// </summary>
        /// <remarks>Byte 29</remarks>
        public readonly byte LanguageDriverId;


        /// <summary>
        /// Reserved. Contains the value 0x00.
        /// </summary>
        /// <remarks>Bytes 30-31</remarks>
        private readonly short EndOfHeaderMarker;

        public DatabaseHeader(ref BufferedBinaryReader reader)
        {
            byte versionInfo = reader.ReadByte();

            Level = (EDatabaseLevel)(versionInfo & 0b0000_0111);
            Version = (EDatabaseVersion)(versionInfo & 0b1111_1000);
            UpdateYear = reader.ReadByte();
            UpdateMonth = reader.ReadByte();
            UpdateDay = reader.ReadByte();
            RecordCount = reader.ReadInt32();
            HeaderLength = reader.ReadInt16();
            RecordLength = reader.ReadInt16();
            Reserved01 = reader.ReadBytes(2);
            IncompleteTransaction = reader.ReadByte();
            EncryptionFlag = reader.ReadByte();
            Reserved02 = reader.ReadBytes(12);
            TableFlags = (ETableFlags)reader.ReadByte();
            LanguageDriverId = reader.ReadByte();
            EndOfHeaderMarker = reader.ReadInt16();
        }
    }
}
