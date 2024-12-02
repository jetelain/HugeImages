using SixLabors.ImageSharp;

namespace Pmad.HugeImages
{
    public interface IHugeImagePartitioner
    {
        List<HugeImagePartDefinition> CreateParts(Size size);
    }
}
