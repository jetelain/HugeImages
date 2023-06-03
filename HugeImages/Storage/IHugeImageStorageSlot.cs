using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HugeImages.Storage
{
    public interface IHugeImageStorageSlot : IDisposable
    {
        Task<Image<TPixel>?> LoadImagePart<TPixel>(int partId)
            where TPixel : unmanaged, IPixel<TPixel>;

        Task SaveImagePart<TPixel>(int partId, Image<TPixel> partImage)
            where TPixel : unmanaged, IPixel<TPixel>;
    }
}
