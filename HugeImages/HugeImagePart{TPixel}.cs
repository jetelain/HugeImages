using System.Diagnostics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace HugeImages
{
    public sealed class HugeImagePart<TPixel> : IDisposable
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly SemaphoreSlim locker = new SemaphoreSlim(1, 1);
        private readonly HugeImage<TPixel> parent;
        private readonly int partId;

        private Image<TPixel>? image;
        private int acquired;
        private bool isOffloading;

        internal HugeImagePart(Rectangle rectangle, Rectangle realRectangle, int partId, HugeImage<TPixel> parent)
        {
            this.partId = partId;
            this.parent = parent;
            Rectangle = rectangle;
            RealRectangle = realRectangle;
        }

        public bool IsLoaded  => image != null;

        internal bool CanOffloadNow => image != null && acquired == 0 && !isOffloading;

        public long LastAccess { get; private set; }

        public Rectangle Rectangle { get; }

        public Rectangle RealRectangle { get; }

        internal bool HasChanged { get; set; }

        private async Task<Image<TPixel>> AcquireImageAsync()
        {
            await locker.WaitAsync().ConfigureAwait(false);
            try
            {
                if (Interlocked.Increment(ref acquired) == 1)
                {
                    // we must acquire the right to lock an image
                    await parent.AcquiredParts.WaitAsync().ConfigureAwait(false);
                }
                if (image == null)
                {
                    image = await parent.LoadImagePart(partId).ConfigureAwait(false);
                    if (image != null && (image.Width != RealRectangle.Width || image.Height != RealRectangle.Height))
                    {
                        throw new IOException("Image size does not match.");
                    }
                    HasChanged = false;
                    if (image == null)
                    {
                        image = new Image<TPixel>(parent.Configuration, RealRectangle.Width, RealRectangle.Height, parent.Background);
                    }
                }
                LastAccess = Stopwatch.GetTimestamp();
            }
            finally
            {
                locker.Release();
            }
            return image;
        }

        internal async Task OffloadAsync()
        {
            await locker.WaitAsync().ConfigureAwait(false);
            try
            {
                if (acquired != 0)
                {
                    throw new InvalidOperationException("This part is still acquired.");
                }
                if (image != null)
                {
                    isOffloading = true;
                    if (HasChanged)
                    {
                        HasChanged = false;
                        await parent.SaveImagePart(partId, image).ConfigureAwait(false);
                    }
                    image.Dispose();
                    parent.LoadedParts.Release();
                    image = null;
                    isOffloading = false;
                }
            }
            finally
            { 
                locker.Release(); 
            }
        }

        public void Dispose()
        {
            var oldimage = Interlocked.Exchange(ref image, null);
            if (oldimage != null)
            {
                oldimage.Dispose();
                parent.LoadedParts.Release();
            }
            locker.Dispose();
        }

        public async Task MutateAsync(Action<IImageProcessingContext> operation)
        {
            using (var token = await AcquireAsync().ConfigureAwait(false))
            {
                token.GetImageReadWrite().Mutate(operation);
            }
        }

        /// <summary>
        /// Acquire a lock on image part to be able to safely use it.
        /// 
        /// This operation can be blocked until a lock on an other part is released, as the number
        /// of simultaneous loaded parts is limited.
        /// 
        /// To release the part, you have to dispose the returned object.
        /// </summary>
        /// <returns></returns>
        public async Task<HugeImagePartToken<TPixel>> AcquireAsync()
        {
            return new HugeImagePartToken<TPixel>(this, await AcquireImageAsync().ConfigureAwait(false));
        }

        internal void Release()
        {
            if (Interlocked.Decrement(ref acquired) == 0)
            {
                // we release the right to lock an image
                parent.AcquiredParts.Release();
            }
        }
    }
}
