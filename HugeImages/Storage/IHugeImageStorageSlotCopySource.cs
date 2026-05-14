namespace Pmad.HugeImages.Storage
{
    /// <summary>
    /// A storage slot that can act as a raw-byte source for copying parts to another storage.
    /// </summary>
    public interface IHugeImageStorageSlotCopySource : IHugeImageStorageSlot
    {
        /// <summary>Returns <c>true</c> if the part identified by <paramref name="partId"/> exists in storage.</summary>
        bool ImagePartExists(int partId);

        /// <summary>Copies the raw encoded bytes of a part to <paramref name="target"/>.</summary>
        /// <param name="partId">Identifier of the part to copy.</param>
        /// <param name="target">Destination stream.</param>
        Task CopyImagePartTo(int partId, Stream target);
    }
}
