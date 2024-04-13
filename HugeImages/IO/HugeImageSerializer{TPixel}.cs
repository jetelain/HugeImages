using System.IO.Compression;
using System.Text.Json;
using HugeImages.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HugeImages.IO
{
    internal static class HugeImageSerializer<TPixel> 
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public static async Task Save(HugeImage<TPixel> himg, Stream stream)
        {
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                var extension = himg.StorageFormat.FileExtensions.First();

                await WriteIndex(himg, archive, extension).ConfigureAwait(false);

                var copyableSlot = himg.Slot as IHugeImageStorageSlotCopyable;
                if (copyableSlot != null)
                {
                    await WritePartsFromCopyable(himg, archive, extension, copyableSlot).ConfigureAwait(false);
                }
                else
                {
                    await WritePartsGeneric(himg, archive, extension).ConfigureAwait(false);
                }
            }
        }

        private static async Task<ZipArchiveEntry> WriteIndex(HugeImage<TPixel> himg, ZipArchive archive, string extension)
        {
            var entry = archive.CreateEntry("index.json", CompressionLevel.Fastest);
            using (var zipStream = entry.Open())
            {
                var index = CreateIndex(himg, extension);
                await JsonSerializer.SerializeAsync(zipStream, index).ConfigureAwait(false);
            }

            return entry;
        }

        private static HugeImageIndex CreateIndex(HugeImage<TPixel> himg, string extension)
        {
            return new HugeImageIndex(
                himg.StorageFormat.DefaultMimeType,
                himg.Size,
                Color.FromPixel(himg.Background),
                himg.Parts.Select(p => new HugeImageIndexPart(p.Rectangle, p.RealRectangle, p.PartId, $"{p.PartId}.{extension}"))
                .ToList());
        }

        private static async Task WritePartsGeneric(HugeImage<TPixel> himg, ZipArchive archive, string extension)
        {
            foreach (var part in himg.Parts)
            {
                var entry = archive.CreateEntry($"{part.PartId}.{extension}", CompressionLevel.NoCompression);
                using (var zipStream = entry.Open())
                {
                    using var token = await part.AcquireAsync().ConfigureAwait(false);
                    await token.GetImageReadOnly().SaveAsync(zipStream, himg.StorageFormat).ConfigureAwait(false);
                }
            }
        }

        private static async Task WritePartsFromCopyable(HugeImage<TPixel> himg, ZipArchive archive, string extension, IHugeImageStorageSlotCopyable fSlot)
        {
            foreach (var part in himg.Parts)
            {
                if (fSlot.ImagePartExists(part.PartId))
                {
                    var entry = archive.CreateEntry($"{part.PartId}.{extension}", CompressionLevel.NoCompression);
                    using (var zipStream = entry.Open())
                    {
                        await part.SaveFromSlot(zipStream, fSlot).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
