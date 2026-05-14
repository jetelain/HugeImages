
namespace Pmad.HugeImages.Storage
{
    /// <summary>
    /// File-system storage that persists image parts under a given base directory.
    /// Parts survive the lifetime of the <see cref="HugeImage{TPixel}"/> instance and can be reloaded in a later session.
    /// </summary>
    public sealed class PersistentHugeImageStorage : HugeImageStorageBase, IHugeImageStorage, IDisposable
    {
        private readonly string basePath;

        /// <summary>
        /// Initialises a new <see cref="PersistentHugeImageStorage"/> rooted at <paramref name="basePath"/>.
        /// </summary>
        /// <param name="basePath">Directory under which image-part files are stored.</param>
        public PersistentHugeImageStorage(string basePath)
        {
            if (basePath == null)
            {
                throw new ArgumentNullException(nameof(basePath));
            }
            this.basePath = basePath;
        }

        internal override HugeImageStorageSlotBase CreateSlot(string name, HugeImageSettingsBase settings)
        {
            return new PersistentHugeImageStorageSlot(Path.Combine(basePath, name), settings);
        }

        public void Dispose()
        {

        }
    }
}
