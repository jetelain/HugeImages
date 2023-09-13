using SixLabors.ImageSharp;

namespace HugeImages
{
    internal class UniqueImagePartitioner : IHugeImagePartitioner
    {
        public List<HugeImagePartDefinition> CreateParts(Size size)
        {
            var rectangle = new Rectangle(Point.Empty, size);
            return new List<HugeImagePartDefinition>() 
            {
                new HugeImagePartDefinition(rectangle, rectangle)
            };
        }
    }
}
