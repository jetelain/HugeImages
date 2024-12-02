using SixLabors.ImageSharp;

namespace Pmad.HugeImages
{
    internal interface IHugeImage : IDisposable
    {
        Size Size { get; }

        IEnumerable<IHugeImagePart> Parts { get; }
    }
}
