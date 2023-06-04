using System.Collections.Concurrent;
using HugeImages.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HugeImages.Test
{
    internal class HugeImageStorageMock : IHugeImageStorageSlot, IHugeImageStorage
    {
        private int readCalls;
        private int writeCalls;

        internal ConcurrentDictionary<int, Image> Storage { get; } = new ConcurrentDictionary<int, Image>();

        internal int ReadCalls => readCalls;

        internal int WriteCalls => writeCalls;

        public IHugeImageStorageSlot CreateSlot(string name, HugeImageSettings settings)
        {
            return this;
        }

        public void Dispose()
        {
            Storage.Clear();
        }

        public Task<Image<TPixel>?> LoadImagePart<TPixel>(int partId) where TPixel : unmanaged, IPixel<TPixel>
        {
            Interlocked.Increment(ref readCalls);
            if (Storage.TryGetValue(partId, out var image))
            {
                return Task.FromResult<Image<TPixel>?>(image.CloneAs<TPixel>());
            }
            return Task.FromResult<Image<TPixel>?>(null);
        }

        public Task SaveImagePart<TPixel>(int partId, Image<TPixel> partImage) where TPixel : unmanaged, IPixel<TPixel>
        {
            Interlocked.Increment(ref writeCalls);
            var clone = partImage.Clone();
            Storage[partId] = clone;
            return Task.CompletedTask;
        }
    }
}
