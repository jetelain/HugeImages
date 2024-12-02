using SixLabors.ImageSharp;

namespace Pmad.HugeImages
{
    internal interface IHugeImagePart
    {
        Rectangle RealRectangle { get; }
    }
}