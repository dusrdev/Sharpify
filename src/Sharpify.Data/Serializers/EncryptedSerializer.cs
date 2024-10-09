using System.Security.Cryptography;

using MemoryPack;

using Sharpify.Collections;

namespace Sharpify.Data.Serializers;

/// <summary>
/// A serializer for a database encryption and case sensitive keys
/// </summary>
internal class EncryptedSerializer : AbstractSerializer {
    protected readonly string _key;

    internal EncryptedSerializer(string path, string key, StringEncoding encoding = StringEncoding.Utf8) : base(path, encoding) {
        _key = key;
    }

/// <inheritdoc />
    internal override Dictionary<string, byte[]?> Deserialize(int estimatedSize) {
        if (estimatedSize is 0) {
            return new Dictionary<string, byte[]?>();
        }

        using var rawBuffer = new RentedBufferWriter<byte>(estimatedSize);
        using var file = new FileStream(_path, FileMode.Open);
        int rawRead = file.Read(rawBuffer.GetSpan());
        rawBuffer.Advance(rawRead);
        scoped ReadOnlySpan<byte> rawSpan = rawBuffer.WrittenSpan;
        using var decryptedBuffer = new RentedBufferWriter<byte>(rawSpan.Length);
        int decryptedRead = Helper.Instance.Decrypt(in rawSpan, decryptedBuffer.GetSpan(), _key);
        decryptedBuffer.Advance(decryptedRead);
        scoped ReadOnlySpan<byte> decrypted = decryptedBuffer.WrittenSpan;
        var dict = MemoryPackSerializer.Deserialize<Dictionary<string, byte[]?>>(decrypted, SerializerOptions);
        return dict ?? new Dictionary<string, byte[]?>();
    }

/// <inheritdoc />
    internal override async ValueTask<Dictionary<string, byte[]?>> DeserializeAsync(int estimatedSize, CancellationToken cancellationToken = default) {
        if (estimatedSize is 0) {
            return new Dictionary<string, byte[]?>();
        }
        using var file = new FileStream(_path, FileMode.Open);
        using var transform = Helper.Instance.GetDecryptor(_key);
        using var cryptoStream = new CryptoStream(file, transform, CryptoStreamMode.Read);
        var dict = await MemoryPackSerializer.DeserializeAsync<Dictionary<string, byte[]?>>(cryptoStream, SerializerOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
        return dict ?? new Dictionary<string, byte[]?>();
    }

/// <inheritdoc />
    internal override void Serialize(Dictionary<string, byte[]?> dict, int estimatedSize) {
        using var buffer = new RentedBufferWriter<byte>(estimatedSize + AesProvider.ReservedBufferSize);
        MemoryPackSerializer.Serialize(buffer, dict, SerializerOptions);
        using var file = new FileStream(_path, FileMode.Create);
        using ICryptoTransform transform = Helper.Instance.GetEncryptor(_key);
        using var cryptoStream = new CryptoStream(file, transform, CryptoStreamMode.Write);
        cryptoStream.Write(buffer.WrittenSpan);
    }

/// <inheritdoc />
    internal override async ValueTask SerializeAsync(Dictionary<string, byte[]?> dict, int estimatedSize, CancellationToken cancellationToken = default) {
        using var file = new FileStream(_path, FileMode.Create);
        using ICryptoTransform transform = Helper.Instance.GetEncryptor(_key);
        using var cryptoStream = new CryptoStream(file, transform, CryptoStreamMode.Write);
        await MemoryPackSerializer.SerializeAsync(cryptoStream, dict, SerializerOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}