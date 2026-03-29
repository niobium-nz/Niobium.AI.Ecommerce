using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Niobium.AI
{
    public class SerializationOptions
    {
        public static JsonSerializerOptions SnakeCase = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        };
    }
}
