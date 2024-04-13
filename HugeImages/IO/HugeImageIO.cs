using HugeImages.Storage;
using SixLabors.ImageSharp.PixelFormats;

namespace HugeImages.IO
{
    public static class HugeImageIO
    {
        public static async Task Save<TPixel>(this HugeImage<TPixel> himg, string path)
                    where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var fs = File.Create(path))
            {
                await Save(himg, fs).ConfigureAwait(false);
            }
        }

        public static Task Save<TPixel>(this HugeImage<TPixel> himg, Stream stream)
                    where TPixel : unmanaged, IPixel<TPixel>
        {
            return HugeImageSerializer<TPixel>.Save(himg, stream);
        }

        public static Task<HugeImage<TPixel>> LoadReadOnlyLocked<TPixel>(string path, HugeImageSettingsBase? settingsBase = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            return LoadReadOnlyLocked<TPixel>(File.OpenRead(path), settingsBase);
        }

        public static Task<HugeImage<TPixel>> LoadReadOnlyLocked<TPixel>(Stream stream, HugeImageSettingsBase? settingsBase = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            return HugeImageDeserializer<TPixel>.LoadReadOnlyLocked(stream, settingsBase);
        }


        public static async Task<HugeImage<TPixel>> LoadClone<TPixel>(string path, IHugeImageStorage storage, HugeImageSettings? settings = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var fs = File.OpenRead(path))
            {
                return await LoadClone<TPixel>(fs, storage, settings).ConfigureAwait(false);
            }
        }

        public static async Task<HugeImage<TPixel>> LoadClone<TPixel>(Stream stream, IHugeImageStorage storage, HugeImageSettings? settings = null)
                    where TPixel : unmanaged, IPixel<TPixel>
        {
            using var himg = await HugeImageDeserializer<TPixel>.LoadReadOnlyLocked(stream, settings).ConfigureAwait(false);
            return await himg.CloneAsync(storage, settings).ConfigureAwait(false);
        }

        public static async Task<HugeImage<TPixel>> LoadClone<TPixel>(string path, IHugeImageStorage storage, string name, HugeImageSettings? settings = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var fs = File.OpenRead(path))
            {
                return await LoadClone<TPixel>(fs, storage, name, settings).ConfigureAwait(false);
            }
        }

        public static async Task<HugeImage<TPixel>> LoadClone<TPixel>(Stream stream, IHugeImageStorage storage, string name, HugeImageSettings? settings = null)
                    where TPixel : unmanaged, IPixel<TPixel>
        {
            using var himg = await HugeImageDeserializer<TPixel>.LoadReadOnlyLocked(stream, settings).ConfigureAwait(false);
            return await himg.CloneAsync(storage, name, settings).ConfigureAwait(false);
        }




        public static async Task<HugeImage<TPixel>> LoadCopy<TPixel>(string path, HugeImageStorageBase storage, HugeImageSettings? settings = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var fs = File.OpenRead(path))
            {
                return await LoadCopy<TPixel>(fs, storage, settings).ConfigureAwait(false);
            }
        }

        public static Task<HugeImage<TPixel>> LoadCopy<TPixel>(Stream stream, HugeImageStorageBase storage, HugeImageSettingsBase? settings = null)
                    where TPixel : unmanaged, IPixel<TPixel>
        {
            return LoadCopy<TPixel>(stream, storage, Guid.NewGuid().ToString(), settings);
        }

        public static async Task<HugeImage<TPixel>> LoadCopy<TPixel>(string path, HugeImageStorageBase storage, string name, HugeImageSettings? settings = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var fs = File.OpenRead(path))
            {
                return await LoadCopy<TPixel>(fs, storage, name, settings).ConfigureAwait(false);
            }
        }

        public static Task<HugeImage<TPixel>> LoadCopy<TPixel>(Stream stream, HugeImageStorageBase storage, string name, HugeImageSettingsBase? settings = null)
                    where TPixel : unmanaged, IPixel<TPixel>
        {
            return HugeImageDeserializer<TPixel>.LoadCopy(stream, storage, name, settings); 
        }
    }
}
