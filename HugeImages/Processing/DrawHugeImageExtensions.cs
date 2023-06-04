using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Drawing;

namespace HugeImages.Processing
{
    public static class DrawHugeImageExtensions
    {
        public static void DrawHugeImage<TPixel>(this IImageProcessingContext target, HugeImage<TPixel> sourceImage, Point sourceLocation, float opacity = 1f)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            target.DrawHugeImageAsync(sourceImage, sourceLocation, opacity).Wait();
        }

        public static Task DrawHugeImageAsync<TPixel>(this IImageProcessingContext target, HugeImage<TPixel> sourceImage, Point sourceLocation, float opacity = 1f)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            if (target is IHugeImageProcessingContext htarget)
            {
                // If target is also an HugeImage, we have to use the full target size, not only the current part size
                return htarget.DrawHugeImageAsync(sourceImage, sourceLocation, Point.Empty, htarget.ImageSize, opacity);
            }
            return target.DrawHugeImageAsync(sourceImage, sourceLocation, Point.Empty, target.GetCurrentSize(), opacity);
        }

        public static void DrawHugeImage<TPixel>(this IImageProcessingContext target, HugeImage<TPixel> sourceImage, Point sourceLocation, Point targetLocation, Size size, float opacity = 1f)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            target.DrawHugeImageAsync(sourceImage, sourceLocation, targetLocation, size, opacity).Wait();
        }

        public static async Task DrawHugeImageAsync<TPixel>(this IImageProcessingContext target, HugeImage<TPixel> sourceImage, Point sourceLocation, Point targetLocation, Size size, float opacity = 1f)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            if (target is IHugeImageProcessingContext htarget)
            {
                // If target is also an HugeImage, we can optimize the operation
                await htarget.DrawHugeImageAsync(sourceImage, sourceLocation, targetLocation, size, opacity);
                return;
            }
            var options = target.GetGraphicsOptions();
            var sourceRectangle = new Rectangle(sourceLocation, size);
            var delta = new Size(sourceLocation.X - targetLocation.X, sourceLocation.Y - targetLocation.Y);
            foreach (var part in sourceImage.PartsLoadedFirst)
            {
                if (part.Rectangle.IntersectsWith(sourceRectangle))
                {
                    var intersection = Rectangle.Intersect(part.Rectangle, sourceRectangle);
                    using (var token = await part.AcquireAsync().ConfigureAwait(false))
                    {
                        target.ApplyProcessor(
                            new DrawImageProcessor(
                                token.GetImageReadOnly(),
                                part.RealRectangle.Location - delta, // Position of image within target coordinates system
                                options.ColorBlendingMode,
                                options.AlphaCompositionMode,
                                opacity),
                            new Rectangle(
                                intersection.Location - delta,  // Position of intersection within target coordinates system
                                intersection.Size));
                    }
                }
            }
        }
    }
}
