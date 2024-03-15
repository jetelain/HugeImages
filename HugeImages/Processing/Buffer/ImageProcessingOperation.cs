using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing.Processors.Drawing;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.ImageSharp.Processing.Processors.Drawing;

namespace HugeImages.Processing.Buffer
{
    internal class ImageProcessingOperation : IImageProcessingOperation
    {
        private readonly IImageProcessor processor;
        private readonly Rectangle bounds;

        internal ImageProcessingOperation(IImageProcessor processor, Rectangle rectangle)
        {
            this.processor = processor;
            bounds = ComputeBounds(processor, rectangle);
        }

        public Rectangle Bounds => bounds;

        private static Rectangle ComputeBounds(IImageProcessor processor, Rectangle rectangle)
        {
            if (processor is FillPathProcessor fillPath)
            {
                return Rectangle.Intersect(ToRectangle(fillPath.Region.Bounds), rectangle);
            }
            if (processor is DrawPathProcessor drawPath)
            {
                var penWidth = (int)MathF.Ceiling(drawPath.Pen.StrokeWidth);
                var bounds = ToRectangle(drawPath.Path.Bounds);
                bounds.Inflate(penWidth, penWidth);
                return Rectangle.Intersect(bounds, rectangle);
            }
            if (processor is DrawImageProcessor drawImage)
            {
                return Rectangle.Intersect(new Rectangle(drawImage.BackgroundLocation, drawImage.ForeGround.Size), rectangle);
            }
            return rectangle;
        }

        private static Rectangle ToRectangle(RectangleF bounds)
        {
            return Rectangle.FromLTRB(
                (int)MathF.Ceiling(bounds.Left),
                (int)MathF.Ceiling(bounds.Top),
                (int)MathF.Floor(bounds.Right),
                (int)MathF.Floor(bounds.Bottom));
        }

        public void ApplyTo(IImageProcessingContext target)
        {
            target.ApplyProcessor(processor, bounds);
        }
    }
}
