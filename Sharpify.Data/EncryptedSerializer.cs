using System.Reflection.Metadata;
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
    internal override Dictionary<string, ReadOnlyMemory<byte>> Deserialize(int estimatedSize) {
        if (estimatedSize is 0) {
            return new Dictionary<string, ReadOnlyMemory<byte>>();
        }

        // var bytes = File.ReadAllBytes(_path);
        // using var buffer = new RentedBufferWriter<byte>(bytes.Length);
        // int numWritten = Helper.Instance.Decrypt(bytes, buffer.GetSpan(), _key);
        // buffer.Advance(numWritten);

        // Dictionary<string, ReadOnlyMemory<byte>> dict =
        //     MemoryPackSerializer.Deserialize<Dictionary<string, ReadOnlyMemory<byte>>>(buffer.WrittenSpan)
        //  ?? new Dictionary<string, ReadOnlyMemory<byte>>();

        // return dict;

        using var buffer = new RentedBufferWriter<byte>(estimatedSize);
        using var file = new FileStream(_path, FileMode.Open);
        using var transform = Helper.Instance.GetDecryptor(_key);
        using var cryptoStream = new CryptoStream(file, transform, CryptoStreamMode.Read);
        var numRead = cryptoStream.Read(buffer.Buffer, 0, estimatedSize - AesProvider.ReservedBufferSize);
        buffer.Advance(numRead);
        Dictionary<string, ReadOnlyMemory<byte>> dict =
            MemoryPackSerializer.Deserialize<Dictionary<string, ReadOnlyMemory<byte>>>(buffer.WrittenSpan)
         ?? new Dictionary<string, ReadOnlyMemory<byte>>();
        return dict;
    }

/// <inheritdoc />
    internal override async ValueTask<Dictionary<string, ReadOnlyMemory<byte>>> DeserializeAsync(int estimatedSize, CancellationToken cancellationToken = default) {
        if (estimatedSize is 0) {
            return new Dictionary<string, ReadOnlyMemory<byte>>();
        }
        using var file = new FileStream(_path, FileMode.Open);
        using var transform = Helper.Instance.GetDecryptor(_key);
        using var cryptoStream = new CryptoStream(file, transform, CryptoStreamMode.Read);
        Dictionary<string, ReadOnlyMemory<byte>> dict =
            await MemoryPackSerializer.DeserializeAsync<Dictionary<string, ReadOnlyMemory<byte>>>(cryptoStream, cancellationToken: cancellationToken)
         ?? new Dictionary<string, ReadOnlyMemory<byte>>();
        return dict;
    }

/// <inheritdoc />
    internal override void Serialize(Dictionary<string, ReadOnlyMemory<byte>> dict, int estimatedSize) {
        using var buffer = new RentedBufferWriter<byte>(estimatedSize + AesProvider.ReservedBufferSize);
        MemoryPackSerializer.Serialize(buffer, dict);
        using var file = new FileStream(_path, FileMode.Create);
        using var transform = Helper.Instance.GetEncryptor(_key);
        using var cryptoStream = new CryptoStream(file, transform, CryptoStreamMode.Write);
        cryptoStream.Write(buffer.WrittenSpan);
        // using var buffer = new RentedBufferWriter<byte>(estimatedSize + AesProvider.ReservedBufferSize);
        // MemoryPackSerializer.Serialize(buffer, dict);
        // var bytes = Helper.Instance.Encrypt(buffer.WrittenSpan, _key);
        // file.Write(bytes);
    }

/// <inheritdoc />
    internal override async ValueTask SerializeAsync(Dictionary<string, ReadOnlyMemory<byte>> dict, int estimatedSize, CancellationToken cancellationToken = default) {
        using var file = new FileStream(_path, FileMode.Create);
        using var transform = Helper.Instance.GetEncryptor(_key);
        using var cryptoStream = new CryptoStream(file, transform, CryptoStreamMode.Write);
        await MemoryPackSerializer.SerializeAsync(cryptoStream, dict, cancellationToken: cancellationToken);
    }
}