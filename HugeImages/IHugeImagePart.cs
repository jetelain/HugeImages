using SixLabors.ImageSharp;

namespace HugeImages
{
    internal interface IHugeImagePart
    {
        Rectangle RealRectangle { get; }
    }
}