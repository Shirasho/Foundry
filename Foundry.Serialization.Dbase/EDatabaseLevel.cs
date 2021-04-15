namespace Foundry.Serialization.Dbase
{
    /// <summary>
    /// The database level.
    /// </summary>
    /// <remarks>
    /// We can't use binary here since it messes up ToString() calls.
    /// </remarks>
    public enum EDatabaseLevel : byte
    {
        /// <summary>
        /// Level 5.
        /// </summary>
        /// <remarks>
        /// Binary = 3
        /// </remarks>
        Five = 3,

        /// <summary>
        /// The latest level.
        /// </summary>
        Latest = Five
    }
}
