using SixLabors.ImageSharp;

namespace Pmad.HugeImages
{
    /// <summary>
    /// Strategy that decides how a <see cref="HugeImage{TPixel}"/> is split into parts.
    /// </summary>
    public interface IHugeImagePartitioner
    {
        /// <summary>
        /// Partitions an image of the given <paramref name="size"/> into a list of part definitions.
        /// </summary>
        /// <param name="size">Total size of the virtual image.</param>
        List<HugeImagePartDefinition> CreateParts(Size size);
    }
}
