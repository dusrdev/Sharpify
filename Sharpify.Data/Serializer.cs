using System.Buffers;

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
            var buffer = ArrayPool<byte>.Shared.Rent(bin.Length + AesProvider.ReservedBufferSize);
            int length = Helper.Instance.Encrypt(bin, buffer, encryptionKey);
            var bytes = new ReadOnlySpan<byte>(buffer, 0, length);
            file.Write(bytes);
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    internal static async Task SerializeAsync<TSerialized>(this Dictionary<string, TSerialized> data, string path, string encryptionKey) {
        ReadOnlyMemory<byte> bin = MemoryPackSerializer.Serialize(data);
        await using var file = new FileStream(path, FileMode.Create);
        if (encryptionKey.Length is 0) {
            await file.WriteAsync(bin);
        } else {
            var buffer = ArrayPool<byte>.Shared.Rent(bin.Length + AesProvider.ReservedBufferSize);
            int length = Helper.Instance.Encrypt(bin.Span, buffer, encryptionKey);
            var bytes = new ReadOnlyMemory<byte>(buffer, 0, length);
            await file.WriteAsync(bytes);
            ArrayPool<byte>.Shared.Return(buffer);
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
        ReadOnlyMemory<byte> bin = new(await File.ReadAllBytesAsync(path, token));
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

            if (encryptionKey.Length is 0) {
                var buffer = bin;

                var formatter = new DictionaryFormatter<string, TSerialized>(options.GetComparer());

                var state = MemoryPackReaderOptionalStatePool.Rent(MemoryPackSerializerOptions.Default);

                var reader = new MemoryPackReader(buffer, state);

                Dictionary<string, TSerialized>? dict = null;

                formatter.Deserialize(ref reader, ref dict!);

                return dict ?? new(options.GetComparer());
            } else {
                var rented = ArrayPool<byte>.Shared.Rent(bin.Length);
                int length = Helper.Instance.Decrypt(bin, rented, encryptionKey);
                ReadOnlySpan<byte> buffer = new(rented, 0, length);

                var formatter = new DictionaryFormatter<string, TSerialized>(options.GetComparer());

                var state = MemoryPackReaderOptionalStatePool.Rent(MemoryPackSerializerOptions.Default);

                var reader = new MemoryPackReader(buffer, state);

                Dictionary<string, TSerialized>? dict = null;

                formatter.Deserialize(ref reader, ref dict!);

                ArrayPool<byte>.Shared.Return(rented);

                return dict ?? new(options.GetComparer());
            }
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