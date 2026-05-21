using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Pmad.HugeImages.Storage
{
    /// <summary>
    /// Represents a named slot in a <see cref="IHugeImageStorage"/> that stores individual image parts.
    /// </summary>
    public interface IHugeImageStorageSlot : IDisposable
    {
        /// <summary>
        /// Loads a single image part from storage, or returns <c>null</c> if the part does not exist yet.
        /// </summary>
        /// <typeparam name="TPixel">Pixel format.</typeparam>
        /// <param name="partId">Identifier of the part to load.</param>
        Task<Image<TPixel>?> LoadImagePart<TPixel>(int partId)
            where TPixel : unmanaged, IPixel<TPixel>;

        /// <summary>
        /// Saves a single image part to storage.
        /// </summary>
        /// <typeparam name="TPixel">Pixel format.</typeparam>
        /// <param name="partId">Identifier of the part to save.</param>
        /// <param name="partImage">Image data for the part.</param>
        Task SaveImagePart<TPixel>(int partId, Image<TPixel> partImage)
            where TPixel : unmanaged, IPixel<TPixel>;
    }
}
