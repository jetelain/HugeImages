using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace HugeImages.Storage
{
    internal abstract class HugeImageStorageSlotBase : IHugeImageStorageSlot, IHugeImageStorageSlotCopySource
    {
        protected readonly string path;
        private readonly string extension;
        private readonly IImageEncoder encoder;
        private readonly IImageDecoder decoder;

        public HugeImageStorageSlotBase(string path, HugeImageSettingsBase settings)
            : this(path, settings.Configuration, settings.StorageFormat)
        {

        }

        public HugeImageStorageSlotBase(string path, Configuration configuration, IImageFormat format)
        {
            this.path = path;
            extension = "." + format.FileExtensions.First();
            encoder = configuration.ImageFormatsManager.GetEncoder(format);
            decoder = configuration.ImageFormatsManager.GetDecoder(format);
            Directory.CreateDirectory(path);
        }

        public async Task CopyImagePartTo(int partId, Stream target)
        {
            var file = Path.Combine(path, partId + extension);
            using var fstream = File.OpenRead(file);
            await fstream.CopyToAsync(target);
        }

        public abstract void Dispose();

        public bool ImagePartExists(int partId)
        {
            return File.Exists(Path.Combine(path, partId + extension));
        }

        public async Task<Image<TPixel>?> LoadImagePart<TPixel>(int partId) where TPixel : unmanaged, IPixel<TPixel>
        {
            var file = Path.Combine(path, partId + extension);
            if (File.Exists(file))
            {
                return await Image.LoadAsync<TPixel>(file); // TODO: decoder
            }
            return null;
        }

        public Task SaveImagePart<TPixel>(int partId, Image<TPixel> partImage) where TPixel : unmanaged, IPixel<TPixel>
        {
            var file = Path.Combine(path, partId + extension);
            return partImage.SaveAsync(file); // TODO: encoder
        }

        public async Task CopyFrom(IHugeImageStorageSlotCopySource other, IEnumerable<int> partIds)
        {
            foreach(var partId in partIds)
            {
                if (other.ImagePartExists(partId))
                {
                    using var target = File.Create(Path.Combine(path, partId + extension));
                    await other.CopyImagePartTo(partId, target);
                }
            }
        }
    }
}
