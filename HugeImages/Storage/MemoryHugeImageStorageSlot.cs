using System.Collections.Concurrent;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Pmad.HugeImages.Storage
{
    /// <summary>
    /// In-Memory slot intend for unit testing
    /// </summary>
    public sealed class MemoryHugeImageStorageSlot : IHugeImageStorageSlot
    {
        internal MemoryHugeImageStorageSlot(string name, HugeImageSettingsBase settings)
        {
            Name = name;
        }

        public ConcurrentDictionary<int, Image> Parts { get; } = new ConcurrentDictionary<int, Image>();
        
        public string Name { get; }

        public void Dispose()
        {
            foreach(var value in Parts.Values)
            {
                value.Dispose();
            }
            Parts.Clear();
        }

        public Task<Image<TPixel>?> LoadImagePart<TPixel>(int partId) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (Parts.TryGetValue(partId, out var image))
            {
                return Task.FromResult<Image<TPixel>?>(image.CloneAs<TPixel>());
            }
            return Task.FromResult<Image<TPixel>?>(null);
        }

        public Task SaveImagePart<TPixel>(int partId, Image<TPixel> partImage) where TPixel : unmanaged, IPixel<TPixel>
        {
            var clone = partImage.Clone();
            Parts[partId] = clone;
            return Task.CompletedTask;
        }
    }
}
