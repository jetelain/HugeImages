using Pmad.HugeImages.Storage;
using SixLabors.ImageSharp.PixelFormats;

namespace Pmad.HugeImages.IO
{
    /// <summary>
    /// Provides save and load operations for the HugeImage archive format (.himg).
    /// </summary>
    public static class HugeImageIO
    {
        /// <summary>Saves a <see cref="HugeImage{TPixel}"/> to a file using the HugeImage archive format.</summary>
        /// <param name="himg">Image to save.</param>
        /// <param name="path">Destination file path.</param>
        public static async Task SaveAsync<TPixel>(this HugeImage<TPixel> himg, string path)
                    where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var fs = File.Create(path))
            {
                await SaveAsync(himg, fs).ConfigureAwait(false);
            }
        }

        /// <summary>Saves a <see cref="HugeImage{TPixel}"/> to a stream using the HugeImage archive format.</summary>
        /// <param name="himg">Image to save.</param>
        /// <param name="stream">Destination stream.</param>
        public static Task SaveAsync<TPixel>(this HugeImage<TPixel> himg, Stream stream)
                    where TPixel : unmanaged, IPixel<TPixel>
        {
            return HugeImageSerializer<TPixel>.Save(himg, stream);
        }

        /// <summary>
        /// Loads a <see cref="HugeImage{TPixel}"/> from a file in read-only mode.
        /// The archive file remains open (locked) for the lifetime of the returned image.
        /// </summary>
        /// <param name="path">Path to the .himg archive file.</param>
        /// <param name="settingsBase">Optional settings; pass <c>null</c> for defaults.</param>
        public static Task<HugeImage<TPixel>> LoadReadOnlyLockedAsync<TPixel>(string path, HugeImageSettingsBase? settingsBase = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            return LoadReadOnlyLockedAsync<TPixel>(File.OpenRead(path), settingsBase);
        }

        /// <summary>
        /// Loads a <see cref="HugeImage{TPixel}"/> from a stream in read-only mode.
        /// The stream remains open for the lifetime of the returned image.
        /// </summary>
        /// <param name="stream">Source stream containing a .himg archive.</param>
        /// <param name="settingsBase">Optional settings; pass <c>null</c> for defaults.</param>
        public static Task<HugeImage<TPixel>> LoadReadOnlyLockedAsync<TPixel>(Stream stream, HugeImageSettingsBase? settingsBase = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            return HugeImageDeserializer<TPixel>.LoadReadOnlyLocked(stream, settingsBase);
        }

        /// <summary>
        /// Loads a mutable copy of a <see cref="HugeImage{TPixel}"/> from a file into <paramref name="storage"/>.
        /// </summary>
        /// <param name="path">Path to the .himg archive file.</param>
        /// <param name="storage">Storage backend that will hold the cloned image parts.</param>
        /// <param name="settings">Optional partitioning and memory settings; pass <c>null</c> for defaults.</param>
        /// <remarks>The clone is not necessarily written to the supplied storage when this method returns; dirty parts may remain loaded in memory until eviction or <see cref="HugeImage{TPixel}.OffloadAsync" /> is called.</remarks>
        public static async Task<HugeImage<TPixel>> LoadCloneAsync<TPixel>(string path, IHugeImageStorage storage, HugeImageSettings? settings = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var fs = File.OpenRead(path))
            {
                return await LoadCloneAsync<TPixel>(fs, storage, settings).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Loads a mutable copy of a <see cref="HugeImage{TPixel}"/> from a stream into <paramref name="storage"/>.
        /// </summary>
        /// <param name="stream">Source stream containing a .himg archive.</param>
        /// <param name="storage">Storage backend that will hold the cloned image parts.</param>
        /// <param name="settings">Optional partitioning and memory settings; pass <c>null</c> for defaults.</param>
        /// <remarks>The clone is not necessarily written to the supplied storage when this method returns; dirty parts may remain loaded in memory until eviction or <see cref="HugeImage{TPixel}.OffloadAsync" /> is called.</remarks>
        public static async Task<HugeImage<TPixel>> LoadCloneAsync<TPixel>(Stream stream, IHugeImageStorage storage, HugeImageSettings? settings = null)
                    where TPixel : unmanaged, IPixel<TPixel>
        {
            using var himg = await HugeImageDeserializer<TPixel>.LoadReadOnlyLocked(stream, settings).ConfigureAwait(false);
            return await himg.CloneAsync(storage, settings).ConfigureAwait(false);
        }

        /// <summary>
        /// Loads a mutable copy of a <see cref="HugeImage{TPixel}"/> from a file into <paramref name="storage"/>
        /// using the supplied <paramref name="name"/> to identify the slot.
        /// </summary>
        /// <param name="path">Path to the .himg archive file.</param>
        /// <param name="storage">Storage backend that will hold the cloned image parts.</param>
        /// <param name="name">Name used to identify the slot in <paramref name="storage"/>.</param>
        /// <param name="settings">Optional partitioning and memory settings; pass <c>null</c> for defaults.</param>
        /// <remarks>The clone is not necessarily written to the supplied storage when this method returns; dirty parts may remain loaded in memory until eviction or <see cref="HugeImage{TPixel}.OffloadAsync" /> is called.</remarks>
        public static async Task<HugeImage<TPixel>> LoadCloneAsync<TPixel>(string path, IHugeImageStorage storage, string name, HugeImageSettings? settings = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var fs = File.OpenRead(path))
            {
                return await LoadCloneAsync<TPixel>(fs, storage, name, settings).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Loads a mutable copy of a <see cref="HugeImage{TPixel}"/> from a stream into <paramref name="storage"/>
        /// using the supplied <paramref name="name"/> to identify the slot.
        /// </summary>
        /// <param name="stream">Source stream containing a .himg archive.</param>
        /// <param name="storage">Storage backend that will hold the cloned image parts.</param>
        /// <param name="name">Name used to identify the slot in <paramref name="storage"/>.</param>
        /// <param name="settings">Optional partitioning and memory settings; pass <c>null</c> for defaults.</param>
        /// <remarks>The clone is not necessarily written to the supplied storage when this method returns; dirty parts may remain loaded in memory until eviction or <see cref="HugeImage{TPixel}.OffloadAsync" /> is called.</remarks>
        public static async Task<HugeImage<TPixel>> LoadCloneAsync<TPixel>(Stream stream, IHugeImageStorage storage, string name, HugeImageSettings? settings = null)
                    where TPixel : unmanaged, IPixel<TPixel>
        {
            using var himg = await HugeImageDeserializer<TPixel>.LoadReadOnlyLocked(stream, settings).ConfigureAwait(false);
            return await himg.CloneAsync(storage, name, settings).ConfigureAwait(false);
        }

        /// <summary>
        /// Loads a <see cref="HugeImage{TPixel}"/> from a file into <paramref name="storage"/> using a raw byte copy,
        /// avoiding a full decode/encode cycle. Requires a storage that implements <see cref="IHugeImageStorageCanCopy"/>.
        /// A random name is assigned to the slot.
        /// </summary>
        /// <param name="path">Path to the .himg archive file.</param>
        /// <param name="storage">Storage backend that supports raw part copying.</param>
        /// <param name="settings">Optional partitioning and memory settings; pass <c>null</c> for defaults.</param>
        public static async Task<HugeImage<TPixel>> LoadCopyAsync<TPixel>(string path, IHugeImageStorageCanCopy storage, HugeImageSettings? settings = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var fs = File.OpenRead(path))
            {
                return await LoadCopyAsync<TPixel>(fs, storage, settings).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Loads a <see cref="HugeImage{TPixel}"/> from a stream into <paramref name="storage"/> using a raw byte copy.
        /// A random name is assigned to the slot.
        /// </summary>
        /// <param name="stream">Source stream containing a .himg archive.</param>
        /// <param name="storage">Storage backend that supports raw part copying.</param>
        /// <param name="settings">Optional settings; pass <c>null</c> for defaults.</param>
        public static Task<HugeImage<TPixel>> LoadCopyAsync<TPixel>(Stream stream, IHugeImageStorageCanCopy storage, HugeImageSettingsBase? settings = null)
                    where TPixel : unmanaged, IPixel<TPixel>
        {
            return LoadCopyAsync<TPixel>(stream, storage, Guid.NewGuid().ToString(), settings);
        }

        /// <summary>
        /// Loads a <see cref="HugeImage{TPixel}"/> from a file into <paramref name="storage"/> using a raw byte copy
        /// and assigns the slot the supplied <paramref name="name"/>.
        /// </summary>
        /// <param name="path">Path to the .himg archive file.</param>
        /// <param name="storage">Storage backend that supports raw part copying.</param>
        /// <param name="name">Name used to identify the slot in <paramref name="storage"/>.</param>
        /// <param name="settings">Optional partitioning and memory settings; pass <c>null</c> for defaults.</param>
        public static async Task<HugeImage<TPixel>> LoadCopyAsync<TPixel>(string path, IHugeImageStorageCanCopy storage, string name, HugeImageSettings? settings = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var fs = File.OpenRead(path))
            {
                return await LoadCopyAsync<TPixel>(fs, storage, name, settings).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Loads a <see cref="HugeImage{TPixel}"/> from a stream into <paramref name="storage"/> using a raw byte copy
        /// and assigns the slot the supplied <paramref name="name"/>.
        /// </summary>
        /// <param name="stream">Source stream containing a .himg archive.</param>
        /// <param name="storage">Storage backend that supports raw part copying.</param>
        /// <param name="name">Name used to identify the slot in <paramref name="storage"/>.</param>
        /// <param name="settings">Optional settings; pass <c>null</c> for defaults.</param>
        public static Task<HugeImage<TPixel>> LoadCopyAsync<TPixel>(Stream stream, IHugeImageStorageCanCopy storage, string name, HugeImageSettingsBase? settings = null)
                    where TPixel : unmanaged, IPixel<TPixel>
        {
            return HugeImageDeserializer<TPixel>.LoadCopy(stream, storage, name, settings); 
        }
    }
}
