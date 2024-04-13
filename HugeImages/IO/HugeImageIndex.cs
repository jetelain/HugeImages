
using System.Text.Json.Serialization;
using HugeImages.IO.Json;
using SixLabors.ImageSharp;

namespace HugeImages.IO
{
    public sealed class HugeImageIndex : IHugeImagePartitioner
    {
        [JsonConstructor]
        public HugeImageIndex(string partMimeType, Size size, Color background, List<HugeImageIndexPart> parts)
        {
            PartMimeType = partMimeType;
            Size = size;
            Parts = parts;
            Background = background;
        }

        public string PartMimeType { get; }

        [JsonConverter(typeof(SizeConverter))]
        public Size Size { get; }

        public List<HugeImageIndexPart> Parts { get; }

        [JsonConverter(typeof(ColorConverter))]
        public Color Background { get; }

        List<HugeImagePartDefinition> IHugeImagePartitioner.CreateParts(Size size)
        {
            if (size != Size)
            {
                throw new ArgumentException();
            }
            return Parts.Select(p => new HugeImagePartDefinition(p.Rectangle, p.RealRectangle, p.PartId)).ToList();
        }
    }
}
