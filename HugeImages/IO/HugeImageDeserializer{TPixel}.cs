using System.IO.Compression;
using System.Text.Json;
using Pmad.HugeImages.IO.Json;
using Pmad.HugeImages.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Pmad.HugeImages.IO
{
    internal static class HugeImageDeserializer<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {

        public static async Task<HugeImage<TPixel>> LoadReadOnlyLocked(Stream stream, HugeImageSettingsBase? settingsBase)
        {
            var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
            var index = await ReadIndex(archive).ConfigureAwait(false);
            var settings = CreateSettings(index, settingsBase);
            var slot = new ReadOnlyZipStorageSlot(archive, index, settings);

            return CreateImage(index, settings, slot);
        }

        internal static async Task<HugeImage<TPixel>> LoadCopy(Stream stream, IHugeImageStorageCanCopy storage, string name, HugeImageSettingsBase? settingsBase)
        {
            var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
            var index = await ReadIndex(archive).ConfigureAwait(false);
            var settings = CreateSettings(index, settingsBase);
            var slot = await storage.CreateCopyFrom(name, settings, new ReadOnlyZipStorageSlot(archive, index, settings), index.Parts.Select(p => p.PartId)).ConfigureAwait(false);

            return CreateImage(index, settings, slot);
        }

        private static HugeImage<TPixel> CreateImage(HugeImageIndex index, HugeImageSettingsBase settings, IHugeImageStorageSlot slot)
        {
            return new HugeImage<TPixel>(slot, index.Size, settings, index, index.Background.ToPixel<TPixel>());
        }

        private static HugeImageSettingsBase CreateSettings(HugeImageIndex index, HugeImageSettingsBase? settingsBase)
        {
            var settings =  new HugeImageSettingsBase()
            {
                Configuration = settingsBase?.Configuration ?? Configuration.Default,
                MemoryLimit = settingsBase?.MemoryLimit ?? HugeImageSettingsBase.DefaultMemoryLimit
            };
            settings.StorageFormat = settings.Configuration.ImageFormats.FirstOrDefault(i => i.DefaultMimeType == index.PartMimeType)
                ?? throw new IOException($"Unknown format '{index.PartMimeType}'");
            return settings;
        }

        private static async Task<HugeImageIndex> ReadIndex(ZipArchive archive)
        {
            var indexEntry = archive.Entries.FirstOrDefault(e => e.Name == "index.json");
            if (indexEntry == null)
            {
                throw new IOException($"index.json is missing.");
            }
            using var stream = indexEntry.Open();
            var data = await JsonSerializer.DeserializeAsync<HugeImageIndex>(stream, HugeImageIndexContext.Default.HugeImageIndex).ConfigureAwait(false);
            if ( data == null)
            {
                throw new IOException($"index.json is corrupted.");
            }
            return data;
        }

    }
}
