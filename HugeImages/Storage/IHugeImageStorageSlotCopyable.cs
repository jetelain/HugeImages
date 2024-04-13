namespace HugeImages.Storage
{
    public interface IHugeImageStorageSlotCopyable : IHugeImageStorageSlot
    {
        bool ImagePartExists(int partId);

        Task CopyImagePartTo(int partId, Stream target);
    }
}
