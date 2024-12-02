using System.Text.Json.Serialization;

namespace Pmad.HugeImages.IO.Json
{
    [JsonSerializable(typeof(HugeImageIndex))]
    [JsonSerializable(typeof(int[]))]
    internal partial class HugeImageIndexContext : JsonSerializerContext
    {
    }
}
