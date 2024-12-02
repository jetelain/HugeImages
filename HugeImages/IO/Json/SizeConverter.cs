using System.Text.Json;
using System.Text.Json.Serialization;
using SixLabors.ImageSharp;

namespace Pmad.HugeImages.IO.Json
{
    internal sealed class SizeConverter : JsonConverter<Size>
    {
        public override Size Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var array = JsonSerializer.Deserialize<int[]>(ref reader, HugeImageIndexContext.Default.Int32Array)!;
            return new Size(array[0], array[1]);
        }

        public override void Write(Utf8JsonWriter writer, Size value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.Width);
            writer.WriteNumberValue(value.Height);
            writer.WriteEndArray();
        }
    }
}