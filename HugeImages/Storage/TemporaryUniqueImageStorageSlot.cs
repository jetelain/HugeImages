using SixLabors.ImageSharp;

namespace Pmad.HugeImages.Storage
{
    internal sealed class TemporaryUniqueImageStorageSlot : UniqueImageStorageSlotBase
    {
        public TemporaryUniqueImageStorageSlot(string extension, Image preloaded, HugeImageSettingsBase settings) 
            : base(Path.Combine(Path.GetTempPath(), "HugeImages", Guid.NewGuid().ToString() + extension), preloaded, settings)
        {
            var tempPath = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }
        }
    }
}
