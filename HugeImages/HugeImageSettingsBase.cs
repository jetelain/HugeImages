using SixLabors.ImageSharp;

namespace HugeImages
{
    public abstract class HugeImageSettingsBase
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
        /// Describes how to split an HugeImage in parts based on it's size
        /// </summary>
        /// <param name="size">Size of the image</param>
        /// <returns></returns>
        public abstract List<HugeImagePartDefinition> CreateParts(Size size);
    }
}
