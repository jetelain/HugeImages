
namespace HugeImages.Storage
{
    public sealed class PersistentHugeImageStorage : HugeImageStorageBase, IHugeImageStorage, IDisposable
    {
        private readonly string basePath;

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
