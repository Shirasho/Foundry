namespace Foundry.Serialization.Ini
{
    /// <summary>
    /// Defines what to do when comments are encountered.
    /// </summary>
    public enum EIniCommentHandling
    {
        /// <summary>
        /// Allow comments within the INI document and treat them as
        /// valid tokens.
        /// </summary>
        Allow,

        /// <summary>
        /// Allow comments within the INI document but ignore them.
        /// </summary>
        Skip
    }
}
