using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sharpify.Data;

internal static class Extensions {
    public static byte[] ToByteArray(this string str) => Encoding.UTF8.GetBytes(str);

    public static string ToUtf8String(this ReadOnlySpan<byte> bytes) => Encoding.UTF8.GetString(bytes);

    public static string Serialize<T>(this T value, JsonSerializerContext jsonSerializerContext) => JsonSerializer.Serialize(value, typeof(T), jsonSerializerContext);

    internal static IEqualityComparer<string>? GetComparer(this DatabaseOptions options) => options.HasFlag(DatabaseOptions.IgnoreKeyCases)
            ? StringComparer.OrdinalIgnoreCase
            : null;
}