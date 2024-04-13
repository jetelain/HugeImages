namespace HugeImages.Storage
{
    public interface IHugeImageStorageCanCopy : IHugeImageStorage
    {
        Task<IHugeImageStorageSlot> CreateCopyFrom(string name, HugeImageSettingsBase settings, IHugeImageStorageSlotCopySource other, IEnumerable<int> partIds);
    }
}
