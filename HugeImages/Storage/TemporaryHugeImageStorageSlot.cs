namespace Pmad.HugeImages.Storage
{
    internal class TemporaryHugeImageStorageSlot : HugeImageStorageSlotBase
    {
        public TemporaryHugeImageStorageSlot(string path, HugeImageSettingsBase settings) : base(path, settings)
        {

        }

        public override void Dispose()
        {
            Directory.Delete(path, true);
        }
    }
}