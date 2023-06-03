using System.Collections.Concurrent;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing.Processors.Drawing;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;

namespace HugeImages.Processing
{
    internal class ImageProcessingBuffer : IImageProcessingContext
    {
        private readonly List<(IImageProcessor, Rectangle?)> buffer = new List<(IImageProcessor, Rectangle?)>();

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

        public IImageProcessingContext ApplyProcessor(IImageProcessor processor, Rectangle rectangle)
        {
            AddBounds(Rectangle.Intersect(GetBounds(processor), rectangle));
            return this;
        }

        private Rectangle GetBounds(IImageProcessor processor)
        {
            if (processor is FillPathProcessor fillPath)
            {
                return ToRectangle(fillPath.Region.Bounds);
            }
            if (processor is DrawPathProcessor drawPath)
            {
                return ToRectangle(drawPath.Path.Bounds);
            }
            return new Rectangle(Point.Empty, Size);
        }

        private Rectangle ToRectangle(RectangleF bounds)
        {
            return Rectangle.FromLTRB(
                (int)MathF.Ceiling(bounds.Left),
                (int)MathF.Ceiling(bounds.Top),
                (int)MathF.Floor(bounds.Right),
                (int)MathF.Floor(bounds.Bottom));
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
            AddBounds(GetBounds(processor));
            buffer.Add(new(processor, null));
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
                if (pair.Item2 != null)
                {
                    target.ApplyProcessor(pair.Item1, pair.Item2.Value);
                }
                else
                {
                    target.ApplyProcessor(pair.Item1);
                }
            }
        }
    }
}