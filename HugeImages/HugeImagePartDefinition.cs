using SixLabors.ImageSharp;

namespace Pmad.HugeImages
{
    public sealed class HugeImagePartDefinition
    {
        public HugeImagePartDefinition(Rectangle rectangle, Rectangle realRectangle)
        {
            if (rectangle.Left < realRectangle.Left ||
                rectangle.Top < realRectangle.Top ||
                rectangle.Right > realRectangle.Right ||
                rectangle.Bottom > realRectangle.Bottom)
            {
                throw new ArgumentException("rectangle must be within realRectangle");
            }
            Rectangle = rectangle;
            RealRectangle = realRectangle;
        }

        internal HugeImagePartDefinition(Rectangle rectangle, Rectangle realRectangle, int partId)
        {
            Rectangle = rectangle;
            RealRectangle = realRectangle;
            PartId = partId;
        }

        internal int? PartId { get; }

        public Rectangle Rectangle { get; }

        public Rectangle RealRectangle { get; }
    }
}
