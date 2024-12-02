using System.IO.Compression;
using Pmad.HugeImages.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace Pmad.HugeImages.IO
{
    internal sealed class ReadOnlyZipStorageSlot : IHugeImageStorageSlot, IHugeImageStorageSlotCopySource
    {
        private readonly ZipArchive archive;
        private readonly HugeImageIndex index;
        private readonly HugeImageSettingsBase settings;
        private readonly IImageDecoder decoder;
        private readonly SemaphoreSlim locker = new SemaphoreSlim(1, 1);

        public ReadOnlyZipStorageSlot(ZipArchive archive, HugeImageIndex index, HugeImageSettingsBase settings)
        {
            this.archive = archive;
            this.index = index;
            this.settings = settings;
            this.decoder = settings.Configuration.ImageFormatsManager.GetDecoder(settings.StorageFormat);
        }

        public async Task CopyImagePartTo(int partId, Stream target)
        {
            var entry = index.Parts.FirstOrDefault(p => p.PartId == partId);
            if (entry == null)
            {
                throw new IOException();
            }
            var zipEntry = archive.GetEntry(entry.FileName);
            if (zipEntry == null)
            {
                throw new IOException();
            }
            await locker.WaitAsync().ConfigureAwait(false);
            try
            {
                using var stream = zipEntry.Open();
                await stream.CopyToAsync(target).ConfigureAwait(false);
            }
            finally
            {
                locker.Release();
            }
        }

        public void Dispose()
        {
            archive.Dispose();
        }

        public bool ImagePartExists(int partId)
        {
            var entry = index.Parts.FirstOrDefault(p => p.PartId == partId);
            if (entry != null)
            {
                return archive.GetEntry(entry.FileName) != null;
            }
            return false;
        }

        public async Task<Image<TPixel>?> LoadImagePart<TPixel>(int partId)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var entry = index.Parts.FirstOrDefault(p => p.PartId == partId);
            if (entry != null)
            {
                var zipEntry = archive.GetEntry(entry.FileName);
                if (zipEntry != null)
                {
                    await locker.WaitAsync().ConfigureAwait(false);
                    try
                    {
                        using var stream = zipEntry.Open();
                        return await decoder.DecodeAsync<TPixel>(new DecoderOptions() { Configuration = settings.Configuration }, stream).ConfigureAwait(false);
                    }
                    finally
                    {
                        locker.Release();
                    }
                }
            }
            return null;
        }

        public Task SaveImagePart<TPixel>(int partId, Image<TPixel> partImage)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            throw new InvalidOperationException("This is image is read-only.");
        }
    }
}