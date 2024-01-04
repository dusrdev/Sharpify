using MemoryPack;
using MemoryPack.Formatters;

namespace Sharpify.Data;

internal static class Serializer {
    internal static void Serialize<TSerialized>(this Dictionary<string, TSerialized> data, string path, string encryptionKey) {
        ReadOnlySpan<byte> bin = MemoryPackSerializer.Serialize(data);
        using var file = new FileStream(path, FileMode.Create);
        if (encryptionKey.Length is 0) {
            file.Write(bin);
        } else {
            ReadOnlySpan<byte> bytes = Helper.Instance.Encrypt(bin, encryptionKey);
            file.Write(bytes);
        }
    }

    internal static async Task SerializeAsync<TSerialized>(this Dictionary<string, TSerialized> data, string path, string encryptionKey) {
        ReadOnlyMemory<byte> bin = MemoryPackSerializer.Serialize(data);
        await using var file = new FileStream(path, FileMode.Create);
        if (encryptionKey.Length is 0) {
            await file.WriteAsync(bin);
        } else {
            ReadOnlyMemory<byte> bytes = Helper.Instance.Encrypt(bin.Span, encryptionKey);
            await file.WriteAsync(bytes);
        }
    }

    internal static Dictionary<string, TSerialized> Deserialize<TSerialized>(
        this string path,
        string encryptionKey,
        DatabaseOptions options = 0) {
        ReadOnlySpan<byte> bin = File.ReadAllBytes(path);
        return DeserializeDict<TSerialized>(bin, options, encryptionKey);
    }

    internal static async Task<Dictionary<string, TSerialized>> DeserializeAsync<TSerialized>(
        this string path,
        string encryptionKey,
        DatabaseOptions options = 0,
        CancellationToken token = default) {
        ReadOnlyMemory<byte> bin = await File.ReadAllBytesAsync(path, token);
        return DeserializeDict<TSerialized>(bin.Span, options, encryptionKey);
    }

    private static Dictionary<string, TSerialized> DeserializeDict<TSerialized>(
        ReadOnlySpan<byte> bin,
        DatabaseOptions options,
        string encryptionKey = "") {
        try {
            if (bin.Length is 0) {
                return new(options.GetComparer());
            }

            ReadOnlySpan<byte> buffer = encryptionKey.Length is 0
                ? bin
                : Helper.Instance.Decrypt(bin, encryptionKey);

            var formatter = new DictionaryFormatter<string, TSerialized>(options.GetComparer());

            var state = MemoryPackReaderOptionalStatePool.Rent(MemoryPackSerializerOptions.Default);

            var reader = new MemoryPackReader(buffer, state);

            Dictionary<string, TSerialized>? dict = null;

            formatter.Deserialize(ref reader, ref dict!);

            return dict ?? new(options.GetComparer());
        } catch {
            return new(options.GetComparer());
        }
    }

    internal static Dictionary<string, TSerialized> Convert<TValue, TSerialized>(this Dictionary<string, TValue> dict, Func<TValue, TSerialized> converter) {
        var newDict = new Dictionary<string, TSerialized>(dict.Comparer);
        if (dict.Count is 0) {
            return newDict;
        }
        foreach (var (k, v) in dict) {
            try {
                newDict[k] = converter(v);
            } catch {
                throw new InvalidOperationException($"Converting the value of key <{k}> failed.");
            }
        }
        return newDict;
    }
}