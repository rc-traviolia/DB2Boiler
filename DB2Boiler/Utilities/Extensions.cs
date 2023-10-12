using System.Text.Json;

namespace DB2Boiler.Utilities
{
    public static class Extensions
    {
        public static string ToIndentedJson<T>(this T objectToSerialize)
        {
            return JsonSerializer.Serialize(objectToSerialize, options: new JsonSerializerOptions() { WriteIndented = true });
        }

        public static T GuaranteeNotNull<T>(this T? objectToCheck)
        {
            if(objectToCheck == null)
            {
                throw new NullReferenceException("You cannot have a null reference here.");
            }

            return objectToCheck;
        }
    }
}
