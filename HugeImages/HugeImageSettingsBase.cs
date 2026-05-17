using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;

namespace Pmad.HugeImages
{
    /// <summary>
    /// Base settings shared by <see cref="HugeImageSettings"/> and lightweight configurations that do not
    /// need partitioning logic (e.g. when wrapping a single existing image).
    /// </summary>
    public class HugeImageSettingsBase
    {
        /// <summary>
        /// Default value for <see cref="MemoryLimit"/> (bytes).
        /// It corresponds to 6 GiB, or with default part size, ~6 parts loaded simultaneously
        /// </summary>
        public const long DefaultMemoryLimit = 6_442_450_944;

        // XXX: Use MemoryCache instead to allow to share quota between instances ?
        /// <summary>
        /// Controls how many image parts may be loaded into RAM simultaneously (bytes).
        /// </summary>
        /// <remarks>
        /// The limit is used exclusively to compute <c>maxLoadedParts = MemoryLimit / (partWidth * partHeight * bytesPerPixel)</c>.
        /// It does <b>not</b> account for:
        /// <list type="bullet">
        ///   <item><description>ImageSharp internal processor buffers (e.g. a Gaussian blur requires a second buffer of the same size, effectively doubling memory for that operation).</description></item>
        ///   <item><description>Thumbnails or other intermediate <see cref="SixLabors.ImageSharp.Image"/> instances created during processing.</description></item>
        ///   <item><description>Encoder/decoder working memory.</description></item>
        ///   <item><description>.NET object overhead, GC metadata, or other runtime allocations.</description></item>
        /// </list>
        /// To stay within a hard physical-memory budget, choose a value noticeably lower than the total available RAM
        /// and leave headroom for the costs listed above.
        /// </remarks>
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
