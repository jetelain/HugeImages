using SixLabors.ImageSharp;

namespace Pmad.HugeImages
{
    /// <summary>
    /// Describes the logical and physical extents of a single image part within a <see cref="HugeImage{TPixel}"/>.
    /// </summary>
    public sealed class HugeImagePartDefinition
    {
        /// <summary>
        /// Initialises a new part definition.
        /// </summary>
        /// <param name="rectangle">Logical area of this part within the virtual image (no overlap).</param>
        /// <param name="realRectangle">Physical area stored on disk, which includes the overlap border.</param>
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

        /// <summary>Logical area of this part within the virtual image (no overlap border).</summary>
        public Rectangle Rectangle { get; }

        /// <summary>Physical area stored on disk, extended by the overlap border on each adjacent edge.</summary>
        public Rectangle RealRectangle { get; }
    }
}
