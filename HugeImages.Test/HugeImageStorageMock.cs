using System.Collections.Concurrent;
using System.Text;
using HugeImages.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace HugeImages.Test
{
    internal class HugeImageStorageMock : IHugeImageStorageSlot, IHugeImageStorage, IHugeImageStorageCanCopy
    {
        private int readCalls;
        private int writeCalls;
        private int copyFromCalls;

        internal ConcurrentDictionary<int, Image> Storage { get; } = new ConcurrentDictionary<int, Image>();

        internal int ReadCalls => readCalls;

        internal int WriteCalls => writeCalls;

        internal int CopyFromCalls => copyFromCalls;

        public async Task<IHugeImageStorageSlot> CreateCopyFrom(string name, HugeImageSettingsBase settings, IHugeImageStorageSlotCopySource other, IEnumerable<int> partIds)
        {
            foreach (var id in partIds)
            {
                if (other.ImagePartExists(id))
                {
                    var mem = new MemoryStream();
                    await other.CopyImagePartTo(id, mem);
                    mem.Position = 0;
                    Storage[id] = PngDecoder.Instance.Decode<Rgb24>(new DecoderOptions(), mem);

                    copyFromCalls++;
                }
            }
            return this;
        }

        public IHugeImageStorageSlot CreateSlot(string name, HugeImageSettingsBase settings)
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
