using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HugeImages.Storage
{
    internal abstract class UniqueImageStorageSlotBase : IHugeImageStorageSlot
    {
        protected readonly string savePath;
        private Image? preloaded;

        public UniqueImageStorageSlotBase(string savePath, Image preloaded, HugeImageSettingsBase settings)
        {
            this.savePath = savePath;
            this.preloaded = preloaded;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            preloaded?.Dispose();
        }

        public Task<Image<TPixel>?> LoadImagePart<TPixel>(int partId) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (preloaded != null)
            {
                return Task.FromResult<Image<TPixel>?>((Image<TPixel>)preloaded);
            }
            return Image.LoadAsync<TPixel>(savePath);
        }

        public Task SaveImagePart<TPixel>(int partId, Image<TPixel> partImage) where TPixel : unmanaged, IPixel<TPixel>
        {
            preloaded = null;
            return partImage.SaveAsync(savePath);
        }
    }
}
