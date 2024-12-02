using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Pmad.HugeImages.Processing.Buffer
{
    internal class DrawHugeImageOperation<TPixel> : IImageProcessingOperation
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly HugeImage<TPixel> sourceImage;
        private readonly Point sourceLocation;
        private readonly Rectangle bounds;
        private readonly float opacity;

        public DrawHugeImageOperation(HugeImage<TPixel> sourceImage, Point sourceLocation, Point targetLocation, Size size, float opacity)
        {
            this.sourceImage = sourceImage;
            this.sourceLocation = sourceLocation;
            this.opacity = opacity;
            this.bounds = new Rectangle(targetLocation, size);
        }

        public Rectangle Bounds => bounds;

        public void ApplyTo(IImageProcessingContext target)
        {
            target.DrawHugeImage(sourceImage, sourceLocation, bounds.Location, bounds.Size, opacity);
        }
    }
}