using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;

namespace HugeImages
{
    public class HugeImageSettings
    {
        /// <summary>
        /// Default value for <see cref="MemoryLimit"/>.
        /// It corresponds to 6 GiB, or with default part size, ~6 parts loaded simultaneously
        /// </summary>
        public const long DefaultMemoryLimit = 6_442_450_944;

        /// <summary>
        /// Default value for <see cref="PartMaxSize"/>.
        /// It corresponds to a 1 GiB image (32bpp).
        /// </summary>
        public const int DefaultPartMaxSize = 16384;

        /// <summary>
        /// Default value for <see cref="PartOverlap"/>
        /// </summary>
        public const int DefaultPartOverlap = 16;

        /// <summary>
        /// Limit value for <see cref="PartMaxSize"/>.
        /// ImageSharp need to be able to compute width * height on a int, so it's equal to Math.Sqrt(int.MaxValue).
        /// It corresponds to a 8GiB image (32bpp), this is not suitable for most system. Default value is more versatile.
        /// </summary>
        public const int PartMaxSizeLimit = 46340; 

        // XXX: Use MemoryCache instead to allow to share quota between instances ?
        /// <summary>
        /// Memory usage limit for each <see cref="HugeImage{TPixel}"/> instance.
        /// </summary>
        public long MemoryLimit { get; set; } = DefaultMemoryLimit;

        /// <summary>
        /// Size limit for each image part (including parts overlap).
        /// </summary>
        public int PartMaxSize { get; set; } = DefaultPartMaxSize;

        /// <summary>
        /// Overlap in pixels between image parts.
        /// 
        /// It's value must be choosen to allow each draw operation to be executed on each part without having to load adjacent parts :
        /// - if you intend to apply a blur effect with a radius of 20, you must pick an overlap that is at least of 20 because the blur processor will need 20 pixels at edge
        /// - if you intend to scale the image of a factor of 1/20, you must pick an overlap that is at least of 20 because resampling will need 20 pixels at edge
        /// 
        /// Warning: No adjacent parts loading is done. If the overlap is too low for used effects some artefacts may appear.
        /// 
        /// PartMaxSize includes overlap. Overlap value must be smaller than PartMaxSize / 4.
        /// </summary>
        public int PartOverlap { get; set; } = DefaultPartOverlap;

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
