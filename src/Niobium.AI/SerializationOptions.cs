using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Niobium.AI
{
    public class SerializationOptions
    {
        public readonly static JsonSerializerOptions SnakeCase = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        };

        public readonly static JsonSerializerOptions Web = JsonSerializerOptions.Web;
    }
}
