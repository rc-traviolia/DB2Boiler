using System.Text.Json;

namespace DB2Boiler.Utilities
{
    public static class Extensions
    {
        public static string ToIndentedJson<T>(this T objectToSerialize)
        {
            return JsonSerializer.Serialize(objectToSerialize, options: new JsonSerializerOptions() { WriteIndented = true });
        }
    }
}
