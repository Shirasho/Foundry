using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Media.Nintendo64.Rom.Disassembly
{
    /// <summary>
    /// Represents a region of data.
    /// </summary>
    public class DataRegion
    {
        /// <summary>
        /// The name of the data region.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The physical start address.
        /// </summary>
        public uint StartAddress { get; }

        /// <summary>
        /// The physical end address.
        /// </summary>
        public uint EndAddress { get; }

        public DataRegion(string name, uint startAddress, uint endAddress)
        {
            Guard.IsNotNullOrWhiteSpace(name, nameof(name));
            Guard.IsGreaterThanOrEqualTo(endAddress, startAddress, nameof(endAddress));

            Name = name;
            StartAddress = startAddress;
            EndAddress = endAddress;
        }

        /// <summary>
        /// Returns whether the specified address exists in this data region.
        /// </summary>
        /// <param name="address">The address to search for.</param>
        public bool ContainsAddress(uint address)
        {
            return StartAddress <= address && address <= EndAddress;
        }
    }
}
