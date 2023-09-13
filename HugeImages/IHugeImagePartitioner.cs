using SixLabors.ImageSharp;

namespace HugeImages
{
    public interface IHugeImagePartitioner
    {
        List<HugeImagePartDefinition> CreateParts(Size size);
    }
}
