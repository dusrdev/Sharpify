using System.Buffers;

namespace Sharpify.Data;

/// <summary>
/// A serializer for a database encryption and case sensitive keys
/// </summary>
internal class IgnoreCaseEncryptedSerializer : EncryptedSerializer {
    internal IgnoreCaseEncryptedSerializer(string path, string key) : base(path, key) {
    }

/// <inheritdoc />
    internal override Dictionary<string, ReadOnlyMemory<byte>> Deserialize() {
        ReadOnlySpan<byte> bin = File.ReadAllBytes(_path);
        if (bin.Length is 0) {
            return new Dictionary<string, ReadOnlyMemory<byte>>(StringComparer.OrdinalIgnoreCase);
        }
        var rented = ArrayPool<byte>.Shared.Rent(bin.Length + AesProvider.ReservedBufferSize);
        int length = Helper.Instance.Decrypt(bin, rented, _key);
        ReadOnlySpan<byte> buffer = new(rented, 0, length);
        var dict = IgnoreCaseSerializer.FromSpan(buffer);
        rented.ReturnBufferToSharedArrayPool();
        return dict;
    }

/// <inheritdoc />
    internal override async ValueTask<Dictionary<string, ReadOnlyMemory<byte>>> DeserializeAsync(CancellationToken cancellationToken = default) {
        ReadOnlyMemory<byte> bin = await File.ReadAllBytesAsync(_path, cancellationToken);
        var rented = ArrayPool<byte>.Shared.Rent(bin.Length + AesProvider.ReservedBufferSize);
        int length = Helper.Instance.Decrypt(bin.Span, rented, _key);
        ReadOnlyMemory<byte> buffer = new(rented, 0, length);
        var dict = IgnoreCaseSerializer.FromSpan(buffer.Span);
        rented.ReturnBufferToSharedArrayPool();
        return dict;
    }
}