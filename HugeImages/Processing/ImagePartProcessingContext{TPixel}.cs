using System.Collections.Concurrent;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Drawing.Processing.Processors.Drawing;
using SixLabors.ImageSharp.Drawing.Processing.Processors.Text;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;

namespace HugeImages.Processing
{
    internal class ImagePartProcessingContext<TPixel> : IImageProcessingContext
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly Image<TPixel> image;
        private readonly Rectangle partRectangle;
        private readonly Matrix3x2 matrix;

        internal ImagePartProcessingContext(Image<TPixel> image, Rectangle partRectangle)
            : this(image, partRectangle, image.GetConfiguration())
        {
        }

        internal ImagePartProcessingContext(Image<TPixel> image, Rectangle partRectangle, Configuration configuration)
        {
            if (image.Width != partRectangle.Width || image.Height != partRectangle.Height)
            {
                throw new ArgumentException();
            }
            this.image = image;
            this.partRectangle = partRectangle;
            matrix = Matrix3x2.CreateTranslation(-partRectangle.X, -partRectangle.Y);
            Configuration = configuration;
        }

        public Configuration Configuration { get; }

        public IDictionary<object, object> Properties { get; } = new ConcurrentDictionary<object, object>();

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

        private IImageProcessingContext DoApplyProcessor(IImageProcessor processor, Rectangle rectangle)
        {
            using (IImageProcessor<TPixel> specificProcessor = TransformProcessor(processor).CreatePixelSpecificProcessor(Configuration, image, rectangle))
            {
                specificProcessor.Execute();
            }
            return this;
        }

        private IImageProcessor TransformProcessor(IImageProcessor processor)
        {
            if (processor is FillPathProcessor fillPath)
            {
                return new FillPathProcessor(fillPath.Options, fillPath.Brush, fillPath.Region.Transform(matrix));
            }
            if (processor is DrawPathProcessor drawPath)
            {
                return new DrawPathProcessor(drawPath.Options, drawPath.Pen, drawPath.Path.Transform(matrix));
            }
            if (processor is ClipPathProcessor || processor is DrawTextProcessor)
            {
                throw new NotSupportedException();
            }
            return processor;
        }

        public IImageProcessingContext ApplyProcessor(IImageProcessor processor)
        {
            return DoApplyProcessor(processor, new Rectangle(Point.Empty, partRectangle.Size));
        }

        public Size GetCurrentSize()
        {
            return partRectangle.Size;
        }
    }
}
