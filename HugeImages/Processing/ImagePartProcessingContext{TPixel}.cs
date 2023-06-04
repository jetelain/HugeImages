using System.Collections.Concurrent;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing.Processors.Drawing;
using SixLabors.ImageSharp.Drawing.Processing.Processors.Text;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.ImageSharp.Processing.Processors.Drawing;

namespace HugeImages.Processing
{
    internal class ImagePartProcessingContext<TPixel> : IImageProcessingContext, IHugeImageProcessingContext
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly Image<TPixel> image;
        private readonly Rectangle partRectangle;
        private readonly Matrix3x2 matrix;

        internal ImagePartProcessingContext(Image<TPixel> image, Rectangle partRectangle, Size imageSize, Configuration configuration)
        {
            if (image.Width != partRectangle.Width || image.Height != partRectangle.Height)
            {
                throw new ArgumentException();
            }
            this.image = image;
            this.partRectangle = partRectangle;
            matrix = Matrix3x2.CreateTranslation(-partRectangle.X, -partRectangle.Y);
            Configuration = configuration;
            ImageSize = imageSize;
        }

        public Configuration Configuration { get; }

        public IDictionary<object, object> Properties { get; } = new ConcurrentDictionary<object, object>();

        public Size ImageSize { get; }

        public IImageProcessingContext ApplyProcessor(IImageProcessor processor, Rectangle drawRectangle)
        {
            var rectangle = Rectangle.Intersect(drawRectangle, partRectangle);
            if (rectangle.IsEmpty)
            {
                // Nothing to do
                return this;
            }
            rectangle.Offset(-drawRectangle.Location);
            return DoApplyProcessor(processor, rectangle);
        }

        private IImageProcessingContext DoApplyProcessor(IImageProcessor processor, Rectangle realRectangle)
        {
            var realProcessor = TransformProcessor(processor, realRectangle);
            if (realProcessor != null)
            {
                using (var specificProcessor = realProcessor.CreatePixelSpecificProcessor(Configuration, image, realRectangle))
                {
                    specificProcessor.Execute();
                }
            }
            return this;
        }

        private IImageProcessor? TransformProcessor(IImageProcessor processor, Rectangle realRectangle)
        {
            if (processor is FillPathProcessor fillPath)
            {
                return new FillPathProcessor(fillPath.Options, fillPath.Brush, fillPath.Region.Transform(matrix));
            }
            if (processor is DrawPathProcessor drawPath)
            {
                return new DrawPathProcessor(drawPath.Options, drawPath.Pen, drawPath.Path.Transform(matrix));
            }
            if (processor is DrawImageProcessor drawImage)
            {
                // DrawImage fails if bounds does not match (whereas FillPathProcessor/DrawPathProcessor silently ignore call)
                var realLocation = TransformPoint(drawImage.Location);
                if (new Rectangle(realLocation, drawImage.Image.Size()).IntersectsWith(realRectangle))
                {
                    return new DrawImageProcessor(drawImage.Image, realLocation, drawImage.ColorBlendingMode, drawImage.AlphaCompositionMode, drawImage.Opacity);
                }
                return null;
            }
            if (processor is ClipPathProcessor || processor is DrawTextProcessor || processor is ICloningImageProcessor)
            {
                throw new NotSupportedException($"Processor {processor.GetType().Name} is not supported on an HugeImage.");
            }
            return processor;
        }

        private Point TransformPoint(Point location)
        {
            location.X -= partRectangle.X;
            location.Y -= partRectangle.Y;
            return location;
        }

        public IImageProcessingContext ApplyProcessor(IImageProcessor processor)
        {
            return DoApplyProcessor(processor, new Rectangle(Point.Empty, partRectangle.Size));
        }

        public Size GetCurrentSize()
        {
            return partRectangle.Size;
        }

        public async Task DrawHugeImageAsync<TPixel2>(HugeImage<TPixel2> sourceImage, Point sourceLocation, Point targetLocation, Size size, float opacity)
            where TPixel2 : unmanaged, IPixel<TPixel2>
        {
            var realTargetRectangle = Rectangle.Intersect(partRectangle, new Rectangle(targetLocation, size));
            if (realTargetRectangle.IsEmpty)
            {
                return;
            }

            // Optimal case : everything is perfectly aligned
            if (sourceLocation == targetLocation)
            {
                var aligned = sourceImage.Parts.FirstOrDefault(p => p.RealRectangle == partRectangle);
                if (aligned != null)
                {
                    using (var token = await aligned.AcquireAsync().ConfigureAwait(false))
                    {
                        image.Mutate(Configuration, d =>
                        {
                            var options = this.GetGraphicsOptions();
                            d.ApplyProcessor(
                                new DrawImageProcessor(
                                    token.GetImageReadOnly(),
                                    Point.Empty, // Position of image within target coordinates system
                                    options.ColorBlendingMode,
                                    options.AlphaCompositionMode,
                                    opacity),
                                new Rectangle(
                                TransformPoint(realTargetRectangle.Location),  // Position of intersection within target coordinates system
                                realTargetRectangle.Size));
                        });
                    }
                    return;
                }
            }

            // Un-optimized fallback
            var targetShift = new Size(realTargetRectangle.Location.X - targetLocation.X, realTargetRectangle.Location.Y - targetLocation.Y);
            var realSourceLocation = sourceLocation + targetShift;
            await image.MutateAsync(Configuration, async d =>
            {
                await d.DrawHugeImageAsync(sourceImage, realSourceLocation, TransformPoint(realTargetRectangle.Location), realTargetRectangle.Size, opacity);
            });
        }
    }
}
