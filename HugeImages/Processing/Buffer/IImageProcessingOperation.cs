using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Pmad.HugeImages.Processing.Buffer
{
    internal interface IImageProcessingOperation
    {
        Rectangle Bounds { get; }

        void ApplyTo(IImageProcessingContext target);
    }
}
