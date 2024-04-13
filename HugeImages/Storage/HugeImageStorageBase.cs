namespace HugeImages.Storage
{
    public abstract class HugeImageStorageBase : IHugeImageStorage
    {
        internal async Task<IHugeImageStorageSlot> CreateCopyFrom(string name, HugeImageSettingsBase settings, IHugeImageStorageSlotCopyable other, IEnumerable<int> partIds)
        {
            var slot = CreateSlot(name, settings);
            await slot.CopyFrom(other, partIds);
            return slot;
        }

        IHugeImageStorageSlot IHugeImageStorage.CreateSlot(string name, HugeImageSettingsBase settings)
        {
            return CreateSlot(name, settings);
        }

        internal abstract HugeImageStorageSlotBase CreateSlot(string name, HugeImageSettingsBase settings);
    }
}
