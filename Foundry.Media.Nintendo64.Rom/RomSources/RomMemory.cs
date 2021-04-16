using System;
using System.Buffers;
using System.Threading.Tasks;

namespace Foundry.Media.Nintendo64.Rom
{
    /// <summary>
    /// A ROM data source that has been loaded from a region in memory.
    /// </summary>
    public sealed class RomMemory : RomData
    {
        /// <summary>
        /// Whether the ROM file contents can be unloaded from memory and
        /// reliably loaded at a later time.
        /// </summary>
        public override bool CanReload => true;

        public RomMemory(IMemoryOwner<byte> data)
            : base(data)
        {
        }

        public RomMemory(Memory<byte> data)
            : base(new NonAllocatingMemoryOwner(data))
        {

        }

        public RomMemory(ReadOnlyMemory<byte> data)
            : base(new MemoryOwner(data))
        {

        }

        protected override void LoadRomData()
        {
            // Memory ROM data cannot be unloaded without disposing the RomData, so there is no reason
            // to reload from the underlying memory source. SetData() will handle reloading of metadata.
            SetData(Data);
        }

        protected override ValueTask LoadRomDataAsync()
        {
            // Memory ROM data cannot be unloaded without disposing the RomData, so there is no reason
            // to reload from the underlying memory source. SetData() will handle reloading of metadata.
            SetData(Data);

            return ValueTask.CompletedTask;
        }

        private class NonAllocatingMemoryOwner : IMemoryOwner<byte>
        {
            public Memory<byte> Memory { get; }

            public NonAllocatingMemoryOwner(Memory<byte> memory)
            {
                Memory = memory;
            }

            public void Dispose()
            {
                // Somebody else owns this memory. The way we are using this
                // class is not as a traditional memory owner, so we don't actually
                // own this memory and therefore do not want to clear it on the
                // real owner's behalf.
            }
        }

        private class MemoryOwner : IMemoryOwner<byte>
        {
            public Memory<byte> Memory => UnderlyingMemory.Memory;
            private readonly IMemoryOwner<byte> UnderlyingMemory;

            public MemoryOwner(ReadOnlyMemory<byte> memory)
            {
                // The memory is read-only, and the memory owner expects a mutable area
                // of memory. In this case we will create a copy of the data.
                // TODO: Can we redesign this to where this isn't necessary?
                UnderlyingMemory = MemoryPool<byte>.Shared.Rent(memory.Length);
                memory.CopyTo(UnderlyingMemory.Memory);
            }

            public void Dispose()
            {
                UnderlyingMemory.Dispose();
            }
        }
    }
}
