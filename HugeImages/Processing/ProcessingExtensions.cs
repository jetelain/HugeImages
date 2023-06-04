using HugeImages.Processing.Buffer;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Drawing;

namespace HugeImages.Processing
{
    public static class ProcessingExtensions
    {
        public static ValueTask MutateAllAsync<TPixel>(this HugeImage<TPixel> image, Action<IImageProcessingContext> operation)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            return MutateAllAsync(image, d => { operation(d); return ValueTask.CompletedTask; });
        }

        public static async ValueTask MutateAllAsync<TPixel>(this HugeImage<TPixel> image, Func<IImageProcessingContext, ValueTask> operation)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            foreach (var part in image.PartsLoadedFirst)
            {
                using (var token = await part.AcquireAsync().ConfigureAwait(false))
                {
                    await operation(token.CreateProcessingContext()).ConfigureAwait(false);
                }
            }
        }

        public static ValueTask MutateAllParallelAsync<TPixel>(this HugeImage<TPixel> image, Action<IImageProcessingContext> operation)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            return MutateAllParallelAsync(image, d => { operation(d); return ValueTask.CompletedTask; });
        }

        public static async ValueTask MutateAllParallelAsync<TPixel>(this HugeImage<TPixel> image, Func<IImageProcessingContext, ValueTask> operation)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            await Parallel.ForEachAsync(image.PartsLoadedFirst, new ParallelOptions() { MaxDegreeOfParallelism = image.MaxLoadedPartsCount }, async (part, _) =>
            {
                using (var token = await part.AcquireAsync().ConfigureAwait(false))
                {
                    await operation(token.CreateProcessingContext()).ConfigureAwait(false);
                }
            });
        }

        public static ValueTask MutateAreaAsync<TPixel>(this HugeImage<TPixel> image, Rectangle rectangle, Action<IImageProcessingContext> operation)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            return MutateAreaAsync(image, rectangle, d => { operation(d); return ValueTask.CompletedTask; });
        }

        public static async ValueTask MutateAreaAsync<TPixel>(this HugeImage<TPixel> image, Rectangle rectangle, Func<IImageProcessingContext,ValueTask> operation)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            foreach (var part in image.PartsLoadedFirst)
            {
                if (part.RealRectangle.IntersectsWith(rectangle))
                {
                    using (var token = await part.AcquireAsync().ConfigureAwait(false))
                    {
                        await operation(token.CreateProcessingContext()).ConfigureAwait(false);
                    }
                }
            }
        }

        public static ValueTask MutateAreaParallelAsync<TPixel>(this HugeImage<TPixel> image, Rectangle rectangle, Action<IImageProcessingContext> operation)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            return MutateAreaParallelAsync(image, rectangle, d => { operation(d); return ValueTask.CompletedTask; });
        }

        public static async ValueTask MutateAreaParallelAsync<TPixel>(this HugeImage<TPixel> image, Rectangle rectangle, Func<IImageProcessingContext,ValueTask> operation)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            await Parallel.ForEachAsync(image.PartsLoadedFirst, new ParallelOptions() { MaxDegreeOfParallelism = image.MaxLoadedPartsCount }, async (part, _) =>
            {
                if (part.RealRectangle.IntersectsWith(rectangle))
                {
                    using (var token = await part.AcquireAsync().ConfigureAwait(false))
                    {
                        await operation(token.CreateProcessingContext()).ConfigureAwait(false);
                    }
                }
            });
        }

        public static ValueTask MutateAsync<TPixel>(this HugeImage<TPixel> image, Action<IImageProcessingContext> operation)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            return MutateAsync(image, d => { operation(d); return ValueTask.CompletedTask; });
        }

        public static async ValueTask MutateAsync<TPixel>(this HugeImage<TPixel> image, Func<IImageProcessingContext,ValueTask> operation)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var buffer = new ImageProcessingBuffer(image.Size, image.Configuration);
            await operation(buffer).ConfigureAwait(false);
            await MutateAreaAsync(image, buffer.Bounds, buffer.ApplyTo).ConfigureAwait(false);
            // XXX: We could reduce loaded parts by testing each operation bounds
        }

        public static ValueTask MutateParallelAsync<TPixel>(this HugeImage<TPixel> image, Action<IImageProcessingContext> operation)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            return MutateParallelAsync(image, d => { operation(d); return ValueTask.CompletedTask; });
        }

        public static async ValueTask MutateParallelAsync<TPixel>(this HugeImage<TPixel> image, Func<IImageProcessingContext, ValueTask> operation)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var buffer = new ImageProcessingBuffer(image.Size, image.Configuration);
            await operation(buffer).ConfigureAwait(false);
            await MutateAreaParallelAsync(image, buffer.Bounds, buffer.ApplyTo).ConfigureAwait(false);
            // XXX: We could reduce loaded parts by testing each operation bounds
        }

        public static async Task<Image<TPixel>> ToScaledImageAsync<TPixel>(this HugeImage<TPixel> image, int width, int height)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var scaleX = (double)width / image.Size.Width;
            var scaleY = (double)height / image.Size.Height;
            var scaled = new Image<TPixel>(image.Configuration, width, height);

            foreach (var part in image.PartsLoadedFirst)
            {
                // XXX: DrawImageProcessor does parallel operations, it might be counter-productive to use Parallel.ForEachAsync here
                Image<TPixel> partImage;
                using (var token = await part.AcquireAsync())
                {
                    partImage = token.GetImageReadOnly().Clone(i => i.Resize(Scaled(part.RealRectangle.Size, scaleX, scaleY)));
                }
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

        public static Task MutateAsync<TPixel>(this Image<TPixel> image, Func<IImageProcessingContext,ValueTask> asyncOperation)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            return MutateAsync(image, image.GetConfiguration(), asyncOperation);
        }

        public static async Task MutateAsync<TPixel>(this Image<TPixel> image, Configuration configuration, Func<IImageProcessingContext, ValueTask> asyncOperation)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            IImageProcessingContext? captured = null;
            image.Mutate(configuration, d => { captured = d; });
            // Mutate does basicly :
            //   IInternalImageProcessingContext<TPixel> operationsRunner
            //     = configuration.ImageOperationsProvider.CreateImageProcessingContext(configuration, source, true);
            //   operation(operationsRunner);
            // so we can keep the IImageProcessingContext
            await asyncOperation(captured!).ConfigureAwait(false);
        }
    }
}
