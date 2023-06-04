using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace HugeImages.Processing.Buffer
{
    internal interface IImageProcessingOperation
    {
        Rectangle Bounds { get; }

        void ApplyTo(IImageProcessingContext target);
    }
}
