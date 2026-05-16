namespace Pmad.HugeImages.Storage
{
    /// <summary>
    /// Extended storage interface that supports direct raw-byte copying of image parts from another slot,
    /// avoiding a full decode/encode cycle.
    /// </summary>
    public interface IHugeImageStorageCanCopy : IHugeImageStorage
    {
        /// <summary>
        /// Creates a new storage slot whose parts are copied directly from <paramref name="other"/>,
        /// avoiding a full decode/encode cycle.
        /// </summary>
        /// <param name="name">Name of the new slot.</param>
        /// <param name="settings">Settings for the new slot.</param>
        /// <param name="other">Source slot to copy parts from.</param>
        /// <param name="partIds">Identifiers of the parts to copy.</param>
        Task<IHugeImageStorageSlot> CreateCopyFrom(string name, HugeImageSettingsBase settings, IHugeImageStorageSlotCopySource other, IEnumerable<int> partIds);
    }
}
