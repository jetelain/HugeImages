using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HugeImages.Processing
{
    internal interface IHugeImageProcessingContext
    {
        Size ImageSize { get; }

        Task DrawHugeImageAsync<TPixel>(HugeImage<TPixel> sourceImage, Point sourceLocation, Point targetLocation, Size size, float opacity)
            where TPixel : unmanaged, IPixel<TPixel>;
    }
}
