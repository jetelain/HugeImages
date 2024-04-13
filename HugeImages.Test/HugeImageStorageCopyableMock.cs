using HugeImages.Storage;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;

namespace HugeImages.Test
{
    internal class HugeImageStorageCopyableMock : HugeImageStorageMock, IHugeImageStorageSlotCopySource
    {
        private int copyCalls;

        public IImageEncoder Encoder { get; } = new PngEncoder();

        public int CopyCalls => copyCalls;

        public Task CopyImagePartTo(int partId, Stream target)
        {
            Interlocked.Increment(ref copyCalls);

            return Storage[partId].SaveAsync(target, Encoder);
        }

        public bool ImagePartExists(int partId)
        {
            return Storage.ContainsKey(partId);
        }
    }
}
