using System.Collections.Concurrent;

using MemoryPack;
using MemoryPack.Formatters;

namespace Sharpify.Data;

internal static class Serializer {
    internal static void Serialize<TSerialized>(this ConcurrentDictionary<string, TSerialized> data, string path, string? encryptionKey) {
        ReadOnlySpan<byte> bin = MemoryPackSerializer.Serialize(data);
        using var file = new FileStream(path, FileMode.Create);
        if (string.IsNullOrEmpty(encryptionKey)) {
            file.Write(bin);
        } else {
            ReadOnlySpan<byte> bytes = Helper.Instance.Encrypt(bin, encryptionKey);
        }
        File.WriteAllBytes(path, string.IsNullOrWhiteSpace(encryptionKey) ? bin : bin.Encrypt(encryptionKey));
    }

    internal static async Task SerializeAsync<TSerialized>(this ConcurrentDictionary<string, TSerialized> data, string path, string? encryptionKey) {
        var bin = MemoryPackSerializer.Serialize(data);
        await File.WriteAllBytesAsync(path, string.IsNullOrWhiteSpace(encryptionKey) ? bin : bin.Encrypt(encryptionKey)).ConfigureAwait(false);
    }

    internal static ConcurrentDictionary<string, TSerialized>? Deserialize<TSerialized>(
        this string path,
        string? encryptionKey,
        DatabaseOptions options = 0) {
        var bin = File.ReadAllBytes(path);
        return DeserializeDict<TSerialized>(bin, path, options, encryptionKey);
    }

    internal static async Task<ConcurrentDictionary<string, TSerialized>?> DeserializeAsync<TSerialized>(
        this string path,
        string? encryptionKey,
        DatabaseOptions options = 0,
        CancellationToken token = default) {
        var bin = await File.ReadAllBytesAsync(path, token).ConfigureAwait(false);
        return DeserializeDict<TSerialized>(bin, path, options, encryptionKey);
    }

    private static ConcurrentDictionary<string, TSerialized>? DeserializeDict<TSerialized>(
        byte[] bin,
        string path,
        DatabaseOptions options,
        string? encryptionKey = null) {
        try {
            if (bin.Length is 0) {
                return default;
            }
            var buffer = string.IsNullOrWhiteSpace(encryptionKey)
                ? bin
                : bin.Decrypt(encryptionKey!);

            var formatter = new ConcurrentDictionaryFormatter<string, TSerialized>(options.GetComparer());

            var state = MemoryPackReaderOptionalStatePool.Rent(MemoryPackSerializerOptions.Default);

            var reader = new MemoryPackReader(buffer, state);

            ConcurrentDictionary<string, TSerialized>? dict = null;

#pragma warning disable CS8620 // TSerialized cannot be null anyway.
            formatter.Deserialize(ref reader, ref dict);
#pragma warning restore CS8620

            return dict;
        } catch {
            throw new InvalidDataException($"Could not deserialize the database from <{path}>");
        }
    }

    internal static ConcurrentDictionary<string, TSerialized> Convert<TValue, TSerialized>(this ConcurrentDictionary<string, TValue> dict, Func<TValue, TSerialized> converter) {
        var newDict = new ConcurrentDictionary<string, TSerialized>(dict.Comparer);
        if (dict.IsEmpty) {
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