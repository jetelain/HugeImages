using System.Collections.Concurrent;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;

namespace HugeImages.Processing.Buffer
{
    internal class ImageProcessingBuffer : IImageProcessingContext, IHugeImageProcessingContext
    {
        private readonly List<IImageProcessingOperation> buffer = new List<IImageProcessingOperation>();

        private Rectangle? bounds;

        internal ImageProcessingBuffer(Size size, Configuration configuration)
        {
            Size = size;
            Configuration = configuration;
        }

        public Rectangle Bounds => bounds ?? Rectangle.Empty;

        public Size Size { get; }

        public Configuration Configuration { get; }

        public IDictionary<object, object> Properties { get; } = new ConcurrentDictionary<object, object>();

        public Size ImageSize => Size;

        public IImageProcessingContext ApplyProcessor(IImageProcessor processor, Rectangle rectangle)
        {
            return Add(new ImageProcessingOperation(processor, rectangle));
        }

        private void AddBounds(Rectangle value)
        {
            if (bounds == null)
            {
                bounds = value;
            }
            else
            {
                bounds = Rectangle.Union(value, bounds.Value);
            }
        }

        public IImageProcessingContext ApplyProcessor(IImageProcessor processor)
        {
            return Add(new ImageProcessingOperation(processor, new Rectangle(Point.Empty, Size)));
        }

        private IImageProcessingContext Add(IImageProcessingOperation imageProcessingOperation)
        {
            AddBounds(imageProcessingOperation.Bounds);
            buffer.Add(imageProcessingOperation);
            return this;
        }

        public Size GetCurrentSize()
        {
            return Size;
        }

        public void ApplyTo(IImageProcessingContext target)
        {
            foreach (var pair in buffer)
            {
                pair.ApplyTo(target);
            }
        }

        public Task DrawHugeImageAsync<TPixel>(HugeImage<TPixel> sourceImage, Point sourceLocation, Point targetLocation, Size size, float opacity)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            Add(new DrawHugeImageOperation<TPixel>(sourceImage, sourceLocation, targetLocation, size, opacity));
            return Task.CompletedTask;
        }
    }
}