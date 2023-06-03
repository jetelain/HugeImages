using System.Diagnostics;
using HugeImages.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace HugeImages
{
    public sealed class HugeImagePart<TPixel> : IDisposable
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly HugeImage<TPixel> parent;
        private readonly int partId;

        private Image<TPixel>? image;
        private bool hasChanged;

        internal HugeImagePart(Rectangle rectangle, Rectangle realRectangle, int partId, HugeImage<TPixel> parent)
        {
            this.partId = partId;
            this.parent = parent;
            Rectangle = rectangle;
            RealRectangle = realRectangle;
        }

        public bool IsLoaded  => image != null;

        public long LastAccess { get; set; }

        public Rectangle Rectangle { get; }

        public Rectangle RealRectangle { get; }

        internal async Task<Image<TPixel>> GetImageReadOnly()
        {
            if (image == null)
            {
                image = await parent.LoadImagePart(partId);
                if (image != null && (image.Width != RealRectangle.Width || image.Height != RealRectangle.Height))
                {
                    throw new IOException("Image size does not match.");
                }
                hasChanged = false;
            }
            if (image == null)
            {
                image = new Image<TPixel>(parent.Configuration, RealRectangle.Width, RealRectangle.Height, parent.Background);
                hasChanged = false;
            }
            LastAccess = Stopwatch.GetTimestamp();
            return image;
        }

        public async Task<Image<TPixel>> GetImageReadWrite()
        {
            var image = await GetImageReadOnly();
            hasChanged = true;
            return image;
        }

        internal async Task<IImageProcessingContext> CreateProcessingContext()
        {
            return new ImagePartProcessingContext<TPixel>(await GetImageReadWrite(), RealRectangle, parent.Configuration);
        }
        
        public async Task Offload()
        {
            if (image != null)
            {
                if (hasChanged)
                {
                    await parent.SaveImagePart(partId, image);
                    hasChanged = false;
                }
                image.Dispose();
                image = null;
            }
        }

        public void Dispose()
        {
            if (image != null)
            {
                image.Dispose();
                image = null;
            }
        }

        public async Task Mutate(Action<IImageProcessingContext> operation)
        {
            (await GetImageReadWrite()).Mutate(operation);
        }

    }
}
