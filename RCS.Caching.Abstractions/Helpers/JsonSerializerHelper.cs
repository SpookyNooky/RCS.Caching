using System.Text.Json;

namespace RCS.Caching.Abstractions.Helpers
{
    public static class JsonSerializerHelper
    {
        private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

        public static string Serialize<T>(T obj) => JsonSerializer.Serialize(obj, Options);

        public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);
    }

}
