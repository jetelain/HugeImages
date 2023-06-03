using HugeImages.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace HugeImages
{
    public sealed class HugeImagePartToken<TPixel> : IDisposable
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly Image<TPixel> image;
        private readonly HugeImagePart<TPixel> parent;
        private int isReleased;

        internal HugeImagePartToken(HugeImagePart<TPixel> parent, Image<TPixel> image)
        {
            this.image = image;
            this.parent = parent;
        }

        internal Image<TPixel> GetImageReadOnly()
        {
            return image;
        }

        internal Image<TPixel> GetImageReadWrite()
        {
            parent.HasChanged = true;
            return image;
        }

        internal IImageProcessingContext CreateProcessingContext()
        {
            parent.HasChanged = true;
            return new ImagePartProcessingContext<TPixel>(image, parent.RealRectangle, image.GetConfiguration());
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref isReleased, 1) == 0)
            {
                parent.Release();
            }
        }
    }
}
