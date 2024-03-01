using System.Security.Cryptography;

using Sharpify.Collections;

namespace Sharpify.Data;

/// <summary>
/// A serializer for a database encryption and case sensitive keys
/// </summary>
internal class IgnoreCaseEncryptedSerializer : EncryptedSerializer {
    internal IgnoreCaseEncryptedSerializer(string path, string key) : base(path, key) {
    }

/// <inheritdoc />
    internal override Dictionary<string, byte[]> Deserialize(int estimatedSize) {
        if (estimatedSize is 0) {
            return new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        }
        using var rawBuffer = new RentedBufferWriter<byte>(estimatedSize);
        using var file = new FileStream(_path, FileMode.Open);
        int rawRead = file.Read(rawBuffer.GetSpan());
        rawBuffer.Advance(rawRead);
        var rawSpan = rawBuffer.WrittenSpan;
        using var decryptedBuffer = new RentedBufferWriter<byte>(rawSpan.Length);
        var decryptedRead = Helper.Instance.Decrypt(rawSpan, decryptedBuffer.GetSpan(), _key);
        decryptedBuffer.Advance(decryptedRead);
        var decrypted = decryptedBuffer.WrittenSpan;
        var dict = IgnoreCaseSerializer.FromSpan(decrypted);
        return dict;
    }

/// <inheritdoc />
    internal override async ValueTask<Dictionary<string, byte[]>> DeserializeAsync(int estimatedSize, CancellationToken cancellationToken = default) {
        if (estimatedSize is 0) {
            return new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        }
        using var buffer = new RentedBufferWriter<byte>(estimatedSize);
        using var file = new FileStream(_path, FileMode.Open);
        using var transform = Helper.Instance.GetDecryptor(_key);
        using var cryptoStream = new CryptoStream(file, transform, CryptoStreamMode.Read);
        var numRead = await cryptoStream.ReadAsync(buffer.GetMemory(), cancellationToken).ConfigureAwait(false);
        buffer.Advance(numRead);
        var dict = IgnoreCaseSerializer.FromSpan(buffer.WrittenSpan);
        return dict;
    }
}