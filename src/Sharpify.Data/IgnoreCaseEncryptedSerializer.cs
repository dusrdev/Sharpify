using System.Security.Cryptography;

using MemoryPack;

using Sharpify.Collections;

namespace Sharpify.Data;

/// <summary>
/// A serializer for a database encryption and case-sensitive keys
/// </summary>
internal class IgnoreCaseEncryptedSerializer : EncryptedSerializer {
    internal IgnoreCaseEncryptedSerializer(string path, string key, StringEncoding encoding = StringEncoding.Utf8) : base(path, key, encoding) {
    }

/// <inheritdoc />
    internal override Dictionary<string, byte[]?> Deserialize(int estimatedSize) {
        if (estimatedSize is 0) {
            return new Dictionary<string, byte[]?>(StringComparer.OrdinalIgnoreCase);
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
        Dictionary<string, byte[]?> dict = IgnoreCaseSerializer.FromSpan(in decrypted);
        return dict;
    }

/// <inheritdoc />
    internal override async ValueTask<Dictionary<string, byte[]?>> DeserializeAsync(int estimatedSize, CancellationToken cancellationToken = default) {
        if (estimatedSize is 0) {
            return new Dictionary<string, byte[]?>(StringComparer.OrdinalIgnoreCase);
        }
        using var buffer = new RentedBufferWriter<byte>(estimatedSize);
        using var file = new FileStream(_path, FileMode.Open);
        using ICryptoTransform transform = Helper.Instance.GetDecryptor(_key);
        using var cryptoStream = new CryptoStream(file, transform, CryptoStreamMode.Read);
        int numRead = await cryptoStream.ReadAsync(buffer.GetMemory(), cancellationToken).ConfigureAwait(false);
        buffer.Advance(numRead);
        Dictionary<string, byte[]?> dict = IgnoreCaseSerializer.FromSpan(buffer.WrittenMemory);
        return dict;
    }
}