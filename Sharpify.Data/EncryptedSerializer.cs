using System.Buffers;
using System.Security.Cryptography;

using MemoryPack;

namespace Sharpify.Data;

/// <summary>
/// A serializer for a database encryption and case sensitive keys
/// </summary>
internal class EncryptedSerializer : DatabaseSerializer {
    protected readonly string _key;

    internal EncryptedSerializer(string path, string key) : base(path) {
        _key = key;
    }

/// <inheritdoc />
    internal override Dictionary<string, ReadOnlyMemory<byte>> Deserialize() {
        ReadOnlySpan<byte> bin = File.ReadAllBytes(_path);
        if (bin.Length is 0) {
            return new Dictionary<string, ReadOnlyMemory<byte>>();
        }
        var rented = ArrayPool<byte>.Shared.Rent(bin.Length + AesProvider.ReservedBufferSize);
        int length = Helper.Instance.Decrypt(bin, rented, _key);
        ReadOnlySpan<byte> buffer = new(rented, 0, length);
        Dictionary<string, ReadOnlyMemory<byte>> dict =
            MemoryPackSerializer.Deserialize<Dictionary<string, ReadOnlyMemory<byte>>>(buffer)
         ?? new Dictionary<string, ReadOnlyMemory<byte>>();
        rented.ReturnBufferToSharedArrayPool();
        return dict;
    }

/// <inheritdoc />
    internal override async ValueTask<Dictionary<string, ReadOnlyMemory<byte>>> DeserializeAsync(CancellationToken cancellationToken = default) {
        using var file = new FileStream(_path, FileMode.Open);
        if (file.Length is 0) {
            return new Dictionary<string, ReadOnlyMemory<byte>>();
        }
        using var transform = Helper.Instance.GetDecryptor(_key);
        using var cryptoStream = new CryptoStream(file, transform, CryptoStreamMode.Read);
        Dictionary<string, ReadOnlyMemory<byte>> dict =
            await MemoryPackSerializer.DeserializeAsync<Dictionary<string, ReadOnlyMemory<byte>>>(cryptoStream, cancellationToken: cancellationToken)
         ?? new Dictionary<string, ReadOnlyMemory<byte>>();
        return dict;
    }

/// <inheritdoc />
    internal override void Serialize(Dictionary<string, ReadOnlyMemory<byte>> dict) {
        using var file = new FileStream(_path, FileMode.Create);
        using var transform = Helper.Instance.GetEncryptor(_key);
        using var cryptoStream = new CryptoStream(file, transform, CryptoStreamMode.Write);
        ReadOnlySpan<byte> buffer = MemoryPackSerializer.Serialize(dict);
        cryptoStream.Write(buffer);
    }

/// <inheritdoc />
    internal override async ValueTask SerializeAsync(Dictionary<string, ReadOnlyMemory<byte>> dict, CancellationToken cancellationToken = default) {
        using var file = new FileStream(_path, FileMode.Create);
        using var transform = Helper.Instance.GetEncryptor(_key);
        using var cryptoStream = new CryptoStream(file, transform, CryptoStreamMode.Write);
        await MemoryPackSerializer.SerializeAsync(cryptoStream, dict, cancellationToken: cancellationToken);
    }
}