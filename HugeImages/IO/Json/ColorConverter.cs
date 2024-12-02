using System.Text.Json;
using System.Text.Json.Serialization;
using SixLabors.ImageSharp;

namespace Pmad.HugeImages.IO.Json
{
    internal sealed class ColorConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Color.ParseHex(reader.GetString() ?? "00000000");
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToHex());
        }
    }
}