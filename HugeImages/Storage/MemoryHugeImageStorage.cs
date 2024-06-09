using System.Collections.Concurrent;

namespace HugeImages.Storage
{
    /// <summary>
    /// In-Memory storage intend for unit testing
    /// </summary>
    public sealed class MemoryHugeImageStorage : IHugeImageStorage, IDisposable
    {
        public ConcurrentDictionary<string, MemoryHugeImageStorageSlot> Slots { get; } = new ConcurrentDictionary<string, MemoryHugeImageStorageSlot>();

        public IHugeImageStorageSlot CreateSlot(string name, HugeImageSettingsBase settings)
        {
            return Slots.GetOrAdd(name, n => new MemoryHugeImageStorageSlot(n, settings));
        }

        public void Dispose()
        {
            foreach(var value in Slots.Values)
            {
                value.Dispose();
            }
            Slots.Clear();
        }
    }
}
