using Pmad.HugeImages.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Pmad.HugeImages
{
    /// <summary>
    /// Lock on a <see cref="HugeImagePart{TPixel}"/>, gives raw access to the image part.
    /// </summary>
    /// <typeparam name="TPixel"></typeparam>
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
            if (isReleased == 1)
            {
                throw new ObjectDisposedException(nameof(HugeImagePartToken<TPixel>));
            }
            return image;
        }

        /// <summary>
        /// Get the image part, and mark it as edited.
        /// 
        /// Next offload operation will update mass storage.
        /// </summary>
        /// <returns></returns>
        public Image<TPixel> GetImageReadWrite()
        {
            if (isReleased == 1)
            {
                throw new ObjectDisposedException(nameof(HugeImagePartToken<TPixel>));
            }
            parent.HasChanged = true;
            return image;
        }

        internal IImageProcessingContext CreateProcessingContext()
        {
            return new ImagePartProcessingContext<TPixel>(GetImageReadWrite(), parent.RealRectangle, parent.Parent.Size, image.Configuration);
        }

        /// <summary>
        /// Release the lock on the image. If no other lock have been acquired the image can be unloaded/offloaded at any time.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref isReleased, 1) == 0)
            {
                parent.Release();
            }
        }
    }
}
