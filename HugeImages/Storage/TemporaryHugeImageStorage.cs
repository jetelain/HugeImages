namespace Pmad.HugeImages.Storage
{
    public sealed class TemporaryHugeImageStorage : HugeImageStorageBase, IHugeImageStorage, IDisposable
    {
        private readonly string basePath = Path.Combine(Path.GetTempPath(), "HugeImages", Guid.NewGuid().ToString());

        internal override HugeImageStorageSlotBase CreateSlot(string name, HugeImageSettingsBase settings)
        {
            return new TemporaryHugeImageStorageSlot(Path.Combine(basePath, name), settings);
        }

        public void Dispose()
        {
            Directory.Delete(basePath, true);
        }
    }
}
