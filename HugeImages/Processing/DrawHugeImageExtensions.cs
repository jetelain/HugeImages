using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Drawing;

namespace HugeImages.Processing
{
    public static class DrawHugeImageExtensions
    {
        public static Task DrawHugeImage<TPixel>(this IImageProcessingContext target, HugeImage<TPixel> sourceImage, Point sourceLocation, float opacity = 1f)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            return target.DrawHugeImage(sourceImage, sourceLocation, Point.Empty, target.GetCurrentSize(), opacity);
        }

        public static async Task DrawHugeImage<TPixel>(this IImageProcessingContext target, HugeImage<TPixel> sourceImage, Point sourceLocation, Point targetLocation, Size size, float opacity = 1f)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var options = target.GetGraphicsOptions();
            var sourceRectangle = new Rectangle(sourceLocation, size);
            var delta = new Size(sourceLocation.X - targetLocation.X, sourceLocation.Y - targetLocation.Y);
            foreach (var part in sourceImage.PartsLoadedFirst)
            {
                if (part.Rectangle.IntersectsWith(sourceRectangle))
                {
                    var image = await part.GetImageReadOnly();
                    var intersection = Rectangle.Intersect(part.Rectangle, sourceRectangle);
                    target.ApplyProcessor(
                        new DrawImageProcessor(
                            image,
                            part.RealRectangle.Location - delta, // Position of image within target coordinates system
                            options.ColorBlendingMode,
                            options.AlphaCompositionMode,
                            opacity),
                        new Rectangle(
                            intersection.Location - delta,  // Position of intersection within target coordinates system
                            new Size(intersection.Size)));
                }
            }
        }
    }
}
