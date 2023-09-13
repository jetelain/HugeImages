using HugeImages.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace HugeImages.Storage
{
    public static class StorageExtensions
    {
        public static HugeImage<TPixel> FromUnique<TPixel>(Image<TPixel> image, string extension = ".png")
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var settings = new HugeImageSettingsBase() { Configuration = image.GetConfiguration() };

            return new HugeImage<TPixel>(new TemporaryUniqueImageStorageSlot(extension, image, settings), image.Size(), settings, new UniqueImagePartitioner(), default);
        }

        public static async Task<HugeImage<TPixel>> LoadUniqueAsync<TPixel>(string path)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var image = await Image.LoadAsync<TPixel>(path).ConfigureAwait(false);

            return FromUnique(image, Path.GetExtension(path));
        }

        public static async Task<HugeImage<TPixel>> LoadUniqueReadWriteAsync<TPixel>(string path)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var image = await Image.LoadAsync(path).ConfigureAwait(false);

            var settings = new HugeImageSettingsBase() { Configuration = image.GetConfiguration() };

            return new HugeImage<TPixel>(new PersistentUniqueImageStorageSlot(path, image, settings), image.Size(), settings, new UniqueImagePartitioner(), default);
        }

        public static async Task SaveUniqueAsync<TPixel>(this HugeImage<TPixel> himage, string path)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            if (himage.Parts.Count == 1)
            {
                var part = himage.Parts[0];
                if (part.RealRectangle.X == 0
                    && part.RealRectangle.Y == 0
                    && part.RealRectangle.Width == himage.Size.Width
                    && part.RealRectangle.Height == himage.Size.Height)
                {
                    // Trivial case
                    using (var token = await part.AcquireAsync().ConfigureAwait(false))
                    {
                        await token.GetImageReadOnly().SaveAsync(path).ConfigureAwait(false);
                    }
                    return;
                }
            }
            using (var image = new Image<TPixel>(himage.Size.Width, himage.Size.Height))
            {
                await image.MutateAsync(async (d) =>
                {
                    await d.DrawHugeImageAsync(himage, Point.Empty, 1).ConfigureAwait(false);
                }).ConfigureAwait(false);
                await image.SaveAsync(path).ConfigureAwait(false);
            }
        }
        public static async Task<HugeImage<TPixel>> CloneAsync<TPixel>(this HugeImage<TPixel> himage, IHugeImageStorage storage, HugeImageSettings? settings = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var clone = new HugeImage<TPixel>(storage, himage.Size, settings);
            await clone.MutateAllAsync((d) => d.DrawHugeImageAsync(himage, Point.Empty, 1)).ConfigureAwait(false);
            return clone;
        }

        public static async Task<HugeImage<TPixel>> CloneAsync<TPixel>(this HugeImage<TPixel> himage, IHugeImageStorage storage, string name, HugeImageSettings? settings = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var clone = new HugeImage<TPixel>(storage, name, himage.Size, settings ?? new HugeImageSettings());
            await clone.MutateAllAsync((d) => d.DrawHugeImageAsync(himage, Point.Empty, 1)).ConfigureAwait(false);
            return clone;
        }
    }
}
