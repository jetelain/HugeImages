using Pmad.HugeImages.Storage;
using SixLabors.ImageSharp.PixelFormats;

namespace Pmad.HugeImages.IO
{
    public static class HugeImageIO
    {
        public static async Task SaveAsync<TPixel>(this HugeImage<TPixel> himg, string path)
                    where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var fs = File.Create(path))
            {
                await SaveAsync(himg, fs).ConfigureAwait(false);
            }
        }

        public static Task SaveAsync<TPixel>(this HugeImage<TPixel> himg, Stream stream)
                    where TPixel : unmanaged, IPixel<TPixel>
        {
            return HugeImageSerializer<TPixel>.Save(himg, stream);
        }

        public static Task<HugeImage<TPixel>> LoadReadOnlyLockedAsync<TPixel>(string path, HugeImageSettingsBase? settingsBase = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            return LoadReadOnlyLockedAsync<TPixel>(File.OpenRead(path), settingsBase);
        }

        public static Task<HugeImage<TPixel>> LoadReadOnlyLockedAsync<TPixel>(Stream stream, HugeImageSettingsBase? settingsBase = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            return HugeImageDeserializer<TPixel>.LoadReadOnlyLocked(stream, settingsBase);
        }


        public static async Task<HugeImage<TPixel>> LoadCloneAsync<TPixel>(string path, IHugeImageStorage storage, HugeImageSettings? settings = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var fs = File.OpenRead(path))
            {
                return await LoadCloneAsync<TPixel>(fs, storage, settings).ConfigureAwait(false);
            }
        }

        public static async Task<HugeImage<TPixel>> LoadCloneAsync<TPixel>(Stream stream, IHugeImageStorage storage, HugeImageSettings? settings = null)
                    where TPixel : unmanaged, IPixel<TPixel>
        {
            using var himg = await HugeImageDeserializer<TPixel>.LoadReadOnlyLocked(stream, settings).ConfigureAwait(false);
            return await himg.CloneAsync(storage, settings).ConfigureAwait(false);
        }

        public static async Task<HugeImage<TPixel>> LoadCloneAsync<TPixel>(string path, IHugeImageStorage storage, string name, HugeImageSettings? settings = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var fs = File.OpenRead(path))
            {
                return await LoadCloneAsync<TPixel>(fs, storage, name, settings).ConfigureAwait(false);
            }
        }

        public static async Task<HugeImage<TPixel>> LoadCloneAsync<TPixel>(Stream stream, IHugeImageStorage storage, string name, HugeImageSettings? settings = null)
                    where TPixel : unmanaged, IPixel<TPixel>
        {
            using var himg = await HugeImageDeserializer<TPixel>.LoadReadOnlyLocked(stream, settings).ConfigureAwait(false);
            return await himg.CloneAsync(storage, name, settings).ConfigureAwait(false);
        }




        public static async Task<HugeImage<TPixel>> LoadCopyAsync<TPixel>(string path, IHugeImageStorageCanCopy storage, HugeImageSettings? settings = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var fs = File.OpenRead(path))
            {
                return await LoadCopyAsync<TPixel>(fs, storage, settings).ConfigureAwait(false);
            }
        }

        public static Task<HugeImage<TPixel>> LoadCopyAsync<TPixel>(Stream stream, IHugeImageStorageCanCopy storage, HugeImageSettingsBase? settings = null)
                    where TPixel : unmanaged, IPixel<TPixel>
        {
            return LoadCopyAsync<TPixel>(stream, storage, Guid.NewGuid().ToString(), settings);
        }

        public static async Task<HugeImage<TPixel>> LoadCopyAsync<TPixel>(string path, IHugeImageStorageCanCopy storage, string name, HugeImageSettings? settings = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var fs = File.OpenRead(path))
            {
                return await LoadCopyAsync<TPixel>(fs, storage, name, settings).ConfigureAwait(false);
            }
        }

        public static Task<HugeImage<TPixel>> LoadCopyAsync<TPixel>(Stream stream, IHugeImageStorageCanCopy storage, string name, HugeImageSettingsBase? settings = null)
                    where TPixel : unmanaged, IPixel<TPixel>
        {
            return HugeImageDeserializer<TPixel>.LoadCopy(stream, storage, name, settings); 
        }
    }
}
