using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Drawing;

namespace HugeImages.Processing
{
    public static class ProcessingExtensions
    {
        public static async Task MutateAll<TPixel>(this HugeImage<TPixel> image, Action<IImageProcessingContext> operation)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            foreach (var part in image.PartsLoadedFirst)
            {
                operation(await part.CreateProcessingContext());
            }
        }

        public static async Task MutateAllParallel<TPixel>(this HugeImage<TPixel> image, Action<IImageProcessingContext> operation)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            await Parallel.ForEachAsync(image.PartsLoadedFirst, new ParallelOptions() { MaxDegreeOfParallelism = image.MaxLoadedPartsCount }, async (part, _) =>
            {
                operation(await part.CreateProcessingContext());
            });
        }

        public static async Task Mutate<TPixel>(this HugeImage<TPixel> image, Rectangle rectangle, Action<IImageProcessingContext> operation)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            foreach (var part in image.PartsLoadedFirst)
            {
                if (part.RealRectangle.IntersectsWith(rectangle))
                {
                    operation(await part.CreateProcessingContext());
                }
            }
        }

        public static async Task MutateParallel<TPixel>(this HugeImage<TPixel> image, Rectangle rectangle, Action<IImageProcessingContext> operation)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            await Parallel.ForEachAsync(image.PartsLoadedFirst, new ParallelOptions() { MaxDegreeOfParallelism = image.MaxLoadedPartsCount }, async (part, _) =>
            {
                if (part.RealRectangle.IntersectsWith(rectangle))
                {
                    operation(await part.CreateProcessingContext());
                }
            });
        }

        public static async Task MutateBuffered<TPixel>(this HugeImage<TPixel> image, Action<IImageProcessingContext> operation)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var buffer = new ImageProcessingBuffer(image.Size, image.Configuration);
            operation(buffer);
            await Mutate(image, buffer.Bounds, buffer.ApplyTo);
            // XXX: We could reduce loaded parts by testing each operation bounds
        }

        public static async Task MutateBufferedParallel<TPixel>(this HugeImage<TPixel> image, Action<IImageProcessingContext> operation)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var buffer = new ImageProcessingBuffer(image.Size, image.Configuration);
            operation(buffer);
            await MutateParallel(image, buffer.Bounds, buffer.ApplyTo); 
            // XXX: We could reduce loaded parts by testing each operation bounds
        }

        public static async Task<Image<TPixel>> ToScaledImage<TPixel>(this HugeImage<TPixel> image, int width, int height)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var scaleX = (double)width / image.Size.Width;
            var scaleY = (double)height / image.Size.Height;
            var scaled = new Image<TPixel>(image.Configuration, width, height);

            foreach (var part in image.PartsLoadedFirst) 
            {
                // XXX: DrawImageProcessor does parallel operations, it might be counter-productive to use Parallel.ForEachAsync here

                var partImage = (await part.GetImageReadOnly()).Clone(i => i.Resize(Scaled(part.RealRectangle.Size, scaleX, scaleY)));

                scaled.Mutate(target =>
                {
                    target.ApplyProcessor(
                        new DrawImageProcessor(
                            partImage,
                            Scaled(part.RealRectangle.Location, scaleX, scaleY),
                            PixelColorBlendingMode.Normal,
                            PixelAlphaCompositionMode.SrcOver,
                            1),
                        new Rectangle(
                            Scaled(part.Rectangle.Location, scaleX, scaleY),
                            Scaled(part.Rectangle.Size, scaleX, scaleY)));

                });

            }
            return scaled;
        }

        private static Point Scaled(Point location, double scaleX, double scaleY)
        {
            return new Point((int)Math.Round(location.X * scaleX), (int)Math.Round(location.Y * scaleY));
        }

        private static Size Scaled(Size location, double scaleX, double scaleY)
        {
            return new Size((int)Math.Round(location.Width * scaleX), (int)Math.Round(location.Height * scaleY));
        }
    }
}
