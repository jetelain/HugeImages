namespace Pmad.HugeImages.Storage
{
    /// <summary>
    /// Provides mass storage for <see cref="HugeImage{TPixel}"/> image parts.
    /// </summary>
    public interface IHugeImageStorage
    {
        /// <summary>
        /// Creates or opens a storage slot identified by <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Unique name for the slot within this storage.</param>
        /// <param name="settings">Settings used to configure the slot (image format, memory limit, …).</param>
        /// <returns>An <see cref="IHugeImageStorageSlot"/> that can persist and retrieve image parts.</returns>
        IHugeImageStorageSlot CreateSlot(string name, HugeImageSettingsBase settings);
    }
}
