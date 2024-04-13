using System.Text.Json.Serialization;

namespace HugeImages.IO.Json
{
    [JsonSerializable(typeof(HugeImageIndex))]
    [JsonSerializable(typeof(int[]))]
    internal partial class HugeImageIndexContext : JsonSerializerContext
    {
    }
}
