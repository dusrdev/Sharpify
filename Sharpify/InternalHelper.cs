using System.Text.Json;

namespace Sharpify;

internal static class InternalHelper {
    internal static readonly JsonSerializerOptions JsonOptions = new() {
        WriteIndented = true,
        AllowTrailingCommas = true,
    };

    internal static readonly JsonReaderOptions JsonReaderOptions = new() {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };
}