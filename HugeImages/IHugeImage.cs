using SixLabors.ImageSharp;

namespace HugeImages
{
    internal interface IHugeImage : IDisposable
    {
        Size Size { get; }

        IEnumerable<IHugeImagePart> Parts { get; }
    }
}
