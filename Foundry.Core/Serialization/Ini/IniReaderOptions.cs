namespace Foundry.Serialization.Ini
{
    /// <summary>
    /// Options to use when reading INI files.
    /// </summary>
    public struct IniReaderOptions
    {
        /// <summary>
        /// How to handle comments.
        /// </summary>
        public EIniCommentHandling CommentHandling
        {
            readonly get;
            set;
        }

        /// <summary>
        /// Whether to use the colon (:) as the key-value delimiter instead of an equals
        /// sign (=).
        /// </summary>
        /// <remarks>
        /// The default value is <see langword="false"/>.
        /// </remarks>
        public bool ColonDelimiter { get; set; }

        /// <summary>
        /// Whether duplicate sections with the same case-insensitive name are considered
        /// the same section.
        /// </summary>
        /// <remarks>
        /// The default value is <see langword="false"/>.
        /// </remarks>
        public bool CaseSensitiveSections { get; set; }

        /// <summary>
        /// Whether keys under the same section with the same case-insensitive name are
        /// considered the same key.
        /// </summary>
        /// <remarks>
        /// The default value is <see langword="false"/>.
        /// </remarks>
        public bool CaseSensitiveKeys { get; set; }

        /// <summary>
        /// What to do with a value if a key already exists for that value.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="EIniDuplicateKeyBehavior.Replace"/>.
        /// </remarks>
        public EIniDuplicateKeyBehavior DuplicateKeyBehavior { get; set; }
    }
}
