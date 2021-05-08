using System;

namespace Foundry.Media.Nintendo64.Rom.Disassembly.Internal
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    internal sealed class RestrictionAttribute : Attribute
    {
        /// <summary>
        /// A restriction.
        /// </summary>
        public string Restriction { get; }

        public RestrictionAttribute(string restriction)
        {
            Restriction = restriction;
        }
    }
}
