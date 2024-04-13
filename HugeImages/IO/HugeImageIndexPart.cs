using System.Text.Json.Serialization;
using HugeImages.IO.Json;
using SixLabors.ImageSharp;

namespace HugeImages.IO
{
    public sealed class HugeImageIndexPart
    {
        [JsonConstructor]
        public HugeImageIndexPart(Rectangle rectangle, Rectangle realRectangle, int partId, string fileName)
        {
            Rectangle = rectangle;
            RealRectangle = realRectangle;
            PartId = partId;
            FileName = fileName;
        }

        public int PartId { get; }

        public string FileName { get; }

        [JsonConverter(typeof(RectangleConverter))]
        public Rectangle Rectangle { get; }

        [JsonConverter(typeof(RectangleConverter))]
        public Rectangle RealRectangle { get; }
    }
}