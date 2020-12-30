namespace Foundry.Serialization.Ini
{
    /// <summary>
    /// What to do when a duplicate key is encountered.
    /// </summary>
    public enum EIniDuplicateKeyBehavior
    {
        /// <summary>
        /// Subsequent values for the same key are ignored (the first instance is taken).
        /// </summary>
        Ignore,

        /// <summary>
        /// Subsequent values for the same key overwrite the existing value (the last instance is taken).
        /// </summary>
        Replace,

        /// <summary>
        /// The values are aggregated and returned as a collection.
        /// </summary>
        /// <remarks>
        /// If <see cref="IniSection.GetValue(string)"/> is called, the last element in the collection
        /// will be returned. To return all values, use <see cref="IniSection.GetValues(string)"/> instead.
        /// </remarks>
        Aggregate
    }
}
