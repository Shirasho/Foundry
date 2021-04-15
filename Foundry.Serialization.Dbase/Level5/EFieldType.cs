namespace Foundry.Serialization.Dbase.Level5
{
    /// <summary>
    /// Known DBF data types.
    /// </summary>
    /// <remarks>https://en.wikipedia.org/wiki/.dbf#Level_5_DOS_headers</remarks>
    public enum EFieldType
    {
        /// <summary>
        /// Binary. Stored internally as 10 digits representing a .DBT block number.
        /// </summary>
        Binary = 'B',

        /// <summary>
        /// All code page characters, padded with whitespaces.
        /// </summary>
        Character = 'C',

        /// <summary>
        /// Numbers and a character to separate month, day, and year.
        /// Stored internally as 8 digits in YYYYMMDD format.
        /// </summary>
        Date = 'D',

        /// <summary>
        /// A float value, right justified and padded with whitespaces.
        /// </summary>
        Float = 'F',

        /// <summary>
        /// All code page characters. Stored internally as 10 digits representing a
        /// .DBT block number.
        /// </summary>
        General = 'G',

        /// <summary>
        /// A boolean value. Possible values include 'Y', 'N', 'T', and 'F', case-insensitive.
        /// An uninitialized value is stored as '?'.
        /// </summary>
        Logical = 'L',

        /// <summary>
        /// All code page characters. Stored internally as 10 digits representing
        /// a .DBT block number.
        /// </summary>
        Memo = 'M',

        /// <summary>
        /// A float value, right justified and padded with whitespaces.
        /// </summary>
        Numeric = 'N',

        /// <summary>
        /// A date time, stored as a Julian time.
        /// </summary>
        DateTime = 'T'
    }
}
