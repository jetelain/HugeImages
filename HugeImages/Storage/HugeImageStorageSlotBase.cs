using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace HugeImages.Storage
{
    internal abstract class HugeImageStorageSlotBase : IHugeImageStorageSlot
    {
        protected readonly string path;
        private readonly string extension;
        private readonly IImageEncoder encoder;
        private readonly IImageDecoder decoder;

        public HugeImageStorageSlotBase(string path, HugeImageSettings settings)
            : this(path, settings.Configuration, settings.StorageFormat)
        {

        }

        public HugeImageStorageSlotBase(string path, Configuration configuration, IImageFormat format)
        {
            this.path = path;
            extension = "." + format.FileExtensions.First();
            encoder = configuration.ImageFormatsManager.FindEncoder(format);
            decoder = configuration.ImageFormatsManager.FindDecoder(format);
            Directory.CreateDirectory(path);
        }

        public abstract void Dispose();

        public async Task<Image<TPixel>?> LoadImagePart<TPixel>(int partId) where TPixel : unmanaged, IPixel<TPixel>
        {
            var file = Path.Combine(path, partId + extension);
            if (File.Exists(file))
            {
                return await Image.LoadAsync<TPixel>(file, decoder);
            }
            return null;
        }

        public Task SaveImagePart<TPixel>(int partId, Image<TPixel> partImage) where TPixel : unmanaged, IPixel<TPixel>
        {
            var file = Path.Combine(path, partId + extension);
            return partImage.SaveAsync(file, encoder);
        }
    }
}
