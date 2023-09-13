using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;

namespace HugeImages
{
    public class HugeImageSettingsBase
    {        
        /// <summary>
        /// Default value for <see cref="MemoryLimit"/>.
        /// It corresponds to 6 GiB, or with default part size, ~6 parts loaded simultaneously
        /// </summary>
        public const long DefaultMemoryLimit = 6_442_450_944;

        // XXX: Use MemoryCache instead to allow to share quota between instances ?
        /// <summary>
        /// Memory usage limit for each <see cref="HugeImage{TPixel}"/> instance.
        /// </summary>
        public long MemoryLimit { get; set; } = DefaultMemoryLimit;

        /// <summary>
        /// ImageSharp configuration to use for image
        /// </summary>
        public Configuration Configuration { get; set; } = Configuration.Default;

        /// <summary>
        /// Image format to use for mass storage
        /// </summary>
        public IImageFormat StorageFormat { get; set; } = PngFormat.Instance;
    }
}
