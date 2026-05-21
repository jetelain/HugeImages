namespace Pmad.HugeImages.Storage
{
    /// <summary>
    /// File-system storage that writes image parts to a randomly-named folder inside the system temp directory.
    /// All files are deleted when <see cref="Dispose"/> is called.
    /// </summary>
    public sealed class TemporaryHugeImageStorage : HugeImageStorageBase, IHugeImageStorage, IDisposable
    {
        private readonly string basePath = Path.Combine(Path.GetTempPath(), "HugeImages", Guid.NewGuid().ToString());

        internal string BasePath => basePath;

        internal override HugeImageStorageSlotBase CreateSlot(string name, HugeImageSettingsBase settings)
        {
            return new TemporaryHugeImageStorageSlot(Path.Combine(basePath, name), settings);
        }

        public void Dispose()
        {
            DirectoryHelper.CleanupDirectory(basePath);
        }
    }
}
