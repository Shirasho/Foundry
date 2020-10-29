using System;

namespace Foundry.Reflection
{
    /// <summary>
    /// Defines possible member visibilities.
    /// </summary>
    [Flags]
    public enum EMemberVisibilityFlags : byte
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// Public.
        /// </summary>
        Public = 1,

        /// <summary>
        /// Internal.
        /// </summary>
        Internal = 1 << 1,

        /// <summary>
        /// Protected.
        /// </summary>
        Protected = 1 << 2,

        /// <summary>
        /// Private.
        /// </summary>
        Private = 1 << 3,

        /// <summary>
        /// All visibilities.
        /// </summary>
        All = Public | Internal | Protected | Private
    }
}
