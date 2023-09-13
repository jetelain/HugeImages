using SixLabors.ImageSharp;

namespace HugeImages
{
    public class HugeImageSettings : HugeImageSettingsBase, IHugeImagePartitioner
    {
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

        /// <inheritdoc />
        public List<HugeImagePartDefinition> CreateParts(Size size)
        {
            ValidateSettings();

            var maxSizeWithoutOverlap = PartMaxSize - PartOverlap - PartOverlap;
            var partSize = new Size(GetPartSize(size.Width, maxSizeWithoutOverlap), GetPartSize(size.Height, maxSizeWithoutOverlap));
            var parts = new List<HugeImagePartDefinition>();

            for (var x = 0; x < size.Width; x += partSize.Width)
            {
                for (var y = 0; y < size.Height; y += partSize.Height)
                {
                    var start = new Point(x, y);
                    var partSizeAdjusted = new Size(
                        Math.Min(partSize.Width, size.Width - start.X),
                        Math.Min(partSize.Height, size.Height - start.Y));
                    var realStart = new Point(
                        Math.Max(0, x - PartOverlap),
                        Math.Max(0, y - PartOverlap));
                    var realPartSize = new Size(
                        Math.Min(partSize.Width + PartOverlap, size.Width - realStart.X),
                        Math.Min(partSize.Height + PartOverlap, size.Height - realStart.Y));
                    if (x > 0 && x + partSize.Width < size.Width)
                    {
                        realPartSize.Width += PartOverlap;
                    }
                    if (y > 0 && y + partSize.Height < size.Height)
                    {
                        realPartSize.Height += PartOverlap;
                    }
                    parts.Add(new HugeImagePartDefinition(new Rectangle(start, partSizeAdjusted), new Rectangle(realStart, realPartSize), parts.Count + 1));
                }
            }
            return parts;
        }

        private void ValidateSettings()
        {
            if (PartMaxSize <= 0 || PartMaxSize > PartMaxSizeLimit)
            {
                throw new ArgumentOutOfRangeException("settings", $"PartMaxSize is {PartMaxSize}, it must be greater than 0 and lower than {PartMaxSizeLimit}.");
            }
            if (PartOverlap > PartMaxSize / 4 || PartOverlap <= 0)
            {
                throw new ArgumentOutOfRangeException("settings", $"PartOverlap is {PartOverlap}, it must be between 0 and {PartMaxSize / 4} (PartMaxSize/4).");
            }
        }

        internal static int GetPartSize(double size, double maxSize)
        {
            return (int)Math.Ceiling(size / Math.Ceiling(size / maxSize));
        }
    }
}
