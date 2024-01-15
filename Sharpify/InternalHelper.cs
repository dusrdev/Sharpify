using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sharpify;

internal static class InternalHelper {
    internal static readonly JsonReaderOptions JsonReaderOptions = new() {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<string, string>))]
internal partial class JsonContext : JsonSerializerContext { }