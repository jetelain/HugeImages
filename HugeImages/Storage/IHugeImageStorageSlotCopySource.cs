namespace Pmad.HugeImages.Storage
{
    public interface IHugeImageStorageSlotCopySource : IHugeImageStorageSlot
    {
        bool ImagePartExists(int partId);

        Task CopyImagePartTo(int partId, Stream target);
    }
}
