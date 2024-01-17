using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sharpify.Data;

internal static class Extensions {
    public static string Serialize<T>(this T value, JsonSerializerContext jsonSerializerContext) => JsonSerializer.Serialize(value, typeof(T), jsonSerializerContext);

    internal static IEqualityComparer<string>? GetComparer(this DatabaseOptions options) => options.HasFlag(DatabaseOptions.IgnoreKeyCases)
            ? StringComparer.OrdinalIgnoreCase
            : null;
}