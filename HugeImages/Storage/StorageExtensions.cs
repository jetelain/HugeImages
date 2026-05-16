using Pmad.HugeImages.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Pmad.HugeImages.Storage
{
    /// <summary>
    /// Convenience extension methods for wrapping, cloning and saving a <see cref="HugeImage{TPixel}"/>.
    /// </summary>
    public static class StorageExtensions
    {
        /// <summary>
        /// Wraps an existing <see cref="Image{TPixel}"/> as a single-part <see cref="HugeImage{TPixel}"/> backed by
        /// temporary in-memory storage. The original <paramref name="image"/> is owned by the returned instance.
        /// </summary>
        /// <param name="image">Source image to wrap.</param>
        /// <param name="extension">File extension (including the dot) used to select the storage format, e.g. <c>".png"</c>.</param>
        public static HugeImage<TPixel> FromUnique<TPixel>(Image<TPixel> image, string extension = ".png")
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var settings = new HugeImageSettingsBase() { Configuration = image.Configuration };

            return new HugeImage<TPixel>(new TemporaryUniqueImageStorageSlot(extension, image, settings), image.Size, settings, new UniqueImagePartitioner(), default);
        }

        /// <summary>
        /// Loads a standard image file as a read-only single-part <see cref="HugeImage{TPixel}"/>.
        /// The file format is inferred from the file extension.
        /// </summary>
        /// <param name="path">Path to the image file to load.</param>
        public static async Task<HugeImage<TPixel>> LoadUniqueAsync<TPixel>(string path)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var image = await Image.LoadAsync<TPixel>(path).ConfigureAwait(false);

            return FromUnique(image, Path.GetExtension(path));
        }

        /// <summary>
        /// Loads a standard image file as a read-write single-part <see cref="HugeImage{TPixel}"/> backed by the
        /// original file. Changes are written back to <paramref name="path"/> when the image is saved or offloaded.
        /// </summary>
        /// <param name="path">Path to the image file to load.</param>
        public static async Task<HugeImage<TPixel>> LoadUniqueReadWriteAsync<TPixel>(string path)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var image = await Image.LoadAsync(path).ConfigureAwait(false);

            var settings = new HugeImageSettingsBase() { Configuration = image.Configuration };

            return new HugeImage<TPixel>(new PersistentUniqueImageStorageSlot(path, image, settings), image.Size, settings, new UniqueImagePartitioner(), default);
        }

        /// <summary>
        /// Saves a <see cref="HugeImage{TPixel}"/> to a standard image file.
        /// When the image consists of a single part that covers the full canvas, the part is written directly
        /// without an intermediate copy. Otherwise all parts are composited into a temporary full-size image first.
        /// The output format is inferred from the file extension of <paramref name="path"/>.
        /// </summary>
        /// <param name="himage">Image to save.</param>
        /// <param name="path">Destination file path.</param>
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

        /// <summary>
        /// Creates a mutable deep copy of <paramref name="himage"/> in <paramref name="storage"/>.
        /// A random name is assigned to the new slot.
        /// </summary>
        /// <param name="himage">Source image to clone.</param>
        /// <param name="storage">Storage backend that will hold the cloned image parts.</param>
        /// <param name="settings">Optional partitioning and memory settings; pass <c>null</c> to inherit the defaults.</param>
        /// <remarks>The clone is not necessarily written to the supplied storage when this method returns; dirty parts may remain loaded in memory until eviction or <see cref="HugeImage{TPixel}.OffloadAsync" /> is called.</remarks>
        public static async Task<HugeImage<TPixel>> CloneAsync<TPixel>(this HugeImage<TPixel> himage, IHugeImageStorage storage, HugeImageSettings? settings = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var clone = new HugeImage<TPixel>(storage, himage.Size, settings);
            await clone.MutateAllAsync(async (d) => await d.DrawHugeImageAsync(himage, Point.Empty, 1).ConfigureAwait(false)).ConfigureAwait(false);
            return clone;
        }

        /// <summary>
        /// Creates a mutable deep copy of <paramref name="himage"/> in <paramref name="storage"/>
        /// using the supplied <paramref name="name"/> to identify the slot.
        /// </summary>
        /// <param name="himage">Source image to clone.</param>
        /// <param name="storage">Storage backend that will hold the cloned image parts.</param>
        /// <param name="name">Name used to identify the slot in <paramref name="storage"/>.</param>
        /// <param name="settings">Optional partitioning and memory settings; pass <c>null</c> to inherit the defaults.</param>
        /// <remarks>The clone is not necessarily written to the supplied storage when this method returns; dirty parts may remain loaded in memory until eviction or <see cref="HugeImage{TPixel}.OffloadAsync" /> is called.</remarks>
        public static async Task<HugeImage<TPixel>> CloneAsync<TPixel>(this HugeImage<TPixel> himage, IHugeImageStorage storage, string name, HugeImageSettings? settings = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var clone = new HugeImage<TPixel>(storage, name, himage.Size, settings ?? new HugeImageSettings());
            await clone.MutateAllAsync(async (d) => await d.DrawHugeImageAsync(himage, Point.Empty, 1).ConfigureAwait(false)).ConfigureAwait(false);
            return clone;
        }
    }
}
