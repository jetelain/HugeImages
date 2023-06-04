using System.Runtime.CompilerServices;
using HugeImages.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HugeImages
{
    /// <summary>
    /// Image with extremely large dimensions. Use mass storage to limit memory consumption.
    /// </summary>
    /// <typeparam name="TPixel"></typeparam>
    public sealed class HugeImage<TPixel> : IDisposable, IHugeImage
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly IHugeImageStorageSlot slot;
        private readonly List<HugeImagePart<TPixel>> parts;
        private readonly Size size;
        private readonly int maxLoadedParts;

        public HugeImage(IHugeImageStorage storage, string name, Size size, TPixel background = default)
            : this(storage, name, size, new HugeImageSettings(), background)
        {
        }

        public HugeImage(IHugeImageStorage storage, Size size, HugeImageSettings? settings = null, TPixel background = default)
            : this(storage, Guid.NewGuid().ToString(), size, settings ?? new HugeImageSettings(), background)
        {

        }

        public HugeImage(IHugeImageStorage storage, string name, Size size, HugeImageSettings settings, TPixel background = default)
            : this(storage.CreateSlot(name, settings), size, settings, background)
        {
        }

        private HugeImage(IHugeImageStorageSlot slot, Size size, HugeImageSettings settings, TPixel background)
        {
            if (settings.PartMaxSize <= 0 || settings.PartMaxSize > HugeImageSettings.PartMaxSizeLimit)
            {
                throw new ArgumentOutOfRangeException(nameof(settings), $"PartMaxSize is {settings.PartMaxSize}, it must be greater than 0 and lower than {HugeImageSettings.PartMaxSizeLimit}.");
            }
            if (settings.PartOverlap > settings.PartMaxSize / 4 || settings.PartOverlap <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(settings), $"PartOverlap is {settings.PartOverlap}, it must be between 0 and {settings.PartMaxSize / 4} (PartMaxSize/4).");
            }

            this.slot = slot;
            this.size = size;
            this.parts = CreateParts(size, settings.PartMaxSize, settings.PartOverlap);
            this.maxLoadedParts = Math.Min(parts.Count, ComputeMaxLoadedParts(settings.MemoryLimit, parts.Max(p => p.RealRectangle.Width), parts.Max(p => p.RealRectangle.Height)));
            Configuration = settings.Configuration;
            Background = background;
            AcquiredParts = new SemaphoreSlim(maxLoadedParts, maxLoadedParts);
            LoadedParts = new SemaphoreSlim(maxLoadedParts, maxLoadedParts);
        }

        private static int ComputeMaxLoadedParts(long memoryLimit, long width, long height)
        {
            return Math.Max(1, (int)(memoryLimit / (width * height * Unsafe.SizeOf<TPixel>())));
        }

        /// <summary>
        /// Max number of image parts that can be loaded simultaneously in RAM
        /// </summary>
        public int MaxLoadedPartsCount => maxLoadedParts;

        /// <summary>
        /// Number of image parts currently loaded into RAM
        /// </summary>
        public int LoadedPartsCount => parts.Count(p => p.IsLoaded);

        /// <summary>
        /// Virtual image size
        /// </summary>
        public Size Size => size;

        /// <summary>
        /// Image parts
        /// </summary>
        public IReadOnlyList<HugeImagePart<TPixel>> Parts => parts;

        /// <summary>
        /// Image parts sorted by load state (RAM loaded first)
        /// </summary>
        public IEnumerable<HugeImagePart<TPixel>> PartsLoadedFirst => parts.OrderByDescending(p => p.IsLoaded).ToList();

        internal Configuration Configuration { get; }

        internal TPixel Background { get; }

        internal SemaphoreSlim AcquiredParts { get; }

        internal SemaphoreSlim LoadedParts { get; }

        IEnumerable<IHugeImagePart> IHugeImage.Parts => Parts;

        /// <summary>
        /// Free memory to load one ore more parts
        /// </summary>
        /// <returns></returns>
        internal async Task RequestFreePartsAsync(int count)
        {
            var loaded = parts.Where(p => p.IsLoaded).OrderBy(p => p.LastAccess).ToList();
            if (loaded.Count + count > maxLoadedParts)
            {
                foreach (var part in loaded.Where(u => u.CanOffloadNow).Take(loaded.Count - maxLoadedParts + count))
                {
                    await part.OffloadAsync().ConfigureAwait(false);
                }
            }
        }

        private List<HugeImagePart<TPixel>> CreateParts(Size size, int maxSize, int overlap)
        {
            var maxSizeWithoutOverlap = maxSize - overlap - overlap;
            var partSize = new Size(GetPartSize(size.Width, maxSizeWithoutOverlap), GetPartSize(size.Height, maxSizeWithoutOverlap));
            var parts = new List<HugeImagePart<TPixel>>();

            for (var x = 0; x < size.Width; x += partSize.Width)
            {
                for (var y = 0; y < size.Height; y += partSize.Height)
                {
                    var start = new Point(x, y);
                    var partSizeAdjusted = new Size(
                        Math.Min(partSize.Width, size.Width - start.X),
                        Math.Min(partSize.Height, size.Height - start.Y));
                    var realStart = new Point(
                        Math.Max(0, x - overlap), 
                        Math.Max(0, y - overlap));
                    var realPartSize = new Size(
                        Math.Min(partSize.Width + overlap, size.Width - realStart.X), 
                        Math.Min(partSize.Height + overlap, size.Height - realStart.Y));
                    if ( x > 0 && x + partSize.Width < size.Width)
                    {
                        realPartSize.Width += overlap;
                    }
                    if (y > 0 && y + partSize.Height < size.Height)
                    {
                        realPartSize.Height += overlap;
                    }
                    parts.Add(new HugeImagePart<TPixel>(new Rectangle(start, partSizeAdjusted), new Rectangle(realStart, realPartSize),  parts.Count + 1, this));
                }
            }
            return parts;
        }

        internal static int GetPartSize(double size, double maxSize)
        {
            return (int)Math.Ceiling(size / Math.Ceiling(size / maxSize));
        }

        /// <summary>
        /// Unload all images from RAM, and update mass storage if required.
        /// </summary>
        /// <returns></returns>
        public async Task OffloadAsync()
        {
            await Parallel.ForEachAsync(parts, 
                async (part, _) =>
                {
                    await part.OffloadAsync().ConfigureAwait(false);
                })
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Unload all images from RAM and delete temporary mass storage.
        /// 
        /// Warning : If a persistent mass storage is used, most recent changes will be lost if <see cref="OffloadAsync"/> has not been called before.
        /// </summary>
        public void Dispose()
        {
            foreach (var part in parts)
            {
                part.Dispose();
            }
            slot.Dispose();
            AcquiredParts.Dispose();
            LoadedParts.Dispose();
        }

        internal async Task<Image<TPixel>?> LoadImagePart(int partId)
        {
            do
            {
                await RequestFreePartsAsync(1).ConfigureAwait(false);
            } while (!await LoadedParts.WaitAsync(500).ConfigureAwait(false));

            return await slot.LoadImagePart<TPixel>(partId).ConfigureAwait(false);
        }

        internal Task SaveImagePart(int partId, Image<TPixel> image)
        {
            return slot.SaveImagePart(partId, image);
        }
    }
}
