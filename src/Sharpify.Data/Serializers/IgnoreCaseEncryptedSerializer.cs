using System.Collections.Concurrent;
using System.Security.Cryptography;

using MemoryPack;

using Sharpify.Collections;

namespace Sharpify.Data.Serializers;

/// <summary>
/// A serializer for a database encryption and case-sensitive keys
/// </summary>
internal class IgnoreCaseEncryptedSerializer : EncryptedSerializer {
    internal IgnoreCaseEncryptedSerializer(string path, string key, StringEncoding encoding = StringEncoding.Utf8) : base(path, key, encoding) {
    }

    /// <inheritdoc />
    internal override ConcurrentDictionary<string, byte[]?> Deserialize(int estimatedSize) {
        if (estimatedSize is 0) {
            return new ConcurrentDictionary<string, byte[]?>(StringComparer.OrdinalIgnoreCase);
        }
        using var rawBuffer = new RentedBufferWriter<byte>(estimatedSize);
        using var file = new FileStream(_path, FileMode.Open);
        int rawRead = file.Read(rawBuffer.GetSpan());
        rawBuffer.Advance(rawRead);
        ReadOnlySpan<byte> rawSpan = rawBuffer.WrittenSpan;
        using var decryptedBuffer = new RentedBufferWriter<byte>(rawSpan.Length);
        int decryptedRead = Helper.Instance.Decrypt(rawSpan, decryptedBuffer.GetSpan(), _key);
        decryptedBuffer.Advance(decryptedRead);
        ConcurrentDictionary<string, byte[]?> dict = IgnoreCaseSerializer.FromSpan(decryptedBuffer.WrittenSpan, SerializerOptions);
        return dict;
    }

    /// <inheritdoc />
    internal override async ValueTask<ConcurrentDictionary<string, byte[]?>> DeserializeAsync(int estimatedSize, CancellationToken cancellationToken = default) {
        if (estimatedSize is 0) {
            return new ConcurrentDictionary<string, byte[]?>(StringComparer.OrdinalIgnoreCase);
        }
        using var encryptedBuffer = new RentedBufferWriter<byte>(estimatedSize);
        await using var file = new FileStream(_path, FileMode.Open);
        int encryptedRead = await file.ReadAsync(encryptedBuffer.GetMemory(), cancellationToken);
        encryptedBuffer.Advance(encryptedRead);
        var encrypted = encryptedBuffer.WrittenSpan;
        using var rawBuffer = new RentedBufferWriter<byte>(encrypted.Length);
        int rawRead = Helper.Instance.Decrypt(encrypted, rawBuffer.GetSpan(), _key);
        rawBuffer.Advance(rawRead);
        ConcurrentDictionary<string, byte[]?> dict = IgnoreCaseSerializer.FromSpan(rawBuffer.WrittenSpan, SerializerOptions);
        return dict;
    }
}