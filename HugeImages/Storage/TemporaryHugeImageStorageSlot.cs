namespace HugeImages.Storage
{
    internal class TemporaryHugeImageStorageSlot : HugeImageStorageSlotBase
    {
        public TemporaryHugeImageStorageSlot(string path, HugeImageSettings settings) : base(path, settings)
        {

        }

        public override void Dispose()
        {
            Directory.Delete(path, true);
        }
    }
}