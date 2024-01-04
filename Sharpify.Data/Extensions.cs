using System.Text;
using System.Text.Json;

namespace Sharpify.Data;

internal static class Extensions {
    public static byte[] ToByteArray(this string str) => Encoding.UTF8.GetBytes(str);

    public static string ToUTF8String(this ReadOnlySpan<byte> bytes) => Encoding.UTF8.GetString(bytes);

    public static string Serialize<T>(this T value) => JsonSerializer.Serialize(value);

    internal static IEqualityComparer<string>? GetComparer(this DatabaseOptions options) => options.HasFlag(DatabaseOptions.IgnoreKeyCases)
            ? StringComparer.OrdinalIgnoreCase
            : null;
}