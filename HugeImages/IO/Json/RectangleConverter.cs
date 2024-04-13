using System.Text.Json;
using System.Text.Json.Serialization;
using SixLabors.ImageSharp;

namespace HugeImages.IO.Json
{
    internal sealed class RectangleConverter : JsonConverter<Rectangle>
    {
        public override Rectangle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var array = JsonSerializer.Deserialize<int[]>(ref reader, HugeImageIndexContext.Default.Int32Array)!;
            return new Rectangle(array[0], array[1], array[2], array[3]);
        }

        public override void Write(Utf8JsonWriter writer, Rectangle value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteNumberValue(value.Width);
            writer.WriteNumberValue(value.Height);
            writer.WriteEndArray();
        }
    }
}