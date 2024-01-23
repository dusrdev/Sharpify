using System.Runtime.CompilerServices;

using MemoryPack;

using Sharpify.Collections;

namespace Sharpify.Data;

/// <summary>
/// A serializer for a database without encryption and case sensitive keys
/// </summary>
internal class IgnoreCaseSerializer : Serializer {
    internal IgnoreCaseSerializer(string path) : base(path) {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Dictionary<string, ReadOnlyMemory<byte>> FromSpan(ReadOnlySpan<byte> bin) {
        if (bin.Length is 0) {
            return new Dictionary<string, ReadOnlyMemory<byte>>(StringComparer.OrdinalIgnoreCase);
        }
        var formatter = new OrdinalIgnoreCaseStringDictionaryFormatter<ReadOnlyMemory<byte>>();
        var state = MemoryPackReaderOptionalStatePool.Rent(MemoryPackSerializerOptions.Default);
        var reader = new MemoryPackReader(bin, state);
        Dictionary<string, ReadOnlyMemory<byte>>? dict = null;
        formatter.GetFormatter().Deserialize(ref reader, ref dict!);
        return dict ?? new(StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    internal override Dictionary<string, ReadOnlyMemory<byte>> Deserialize(int estimatedSize) {
        if (estimatedSize is 0) {
            return new Dictionary<string, ReadOnlyMemory<byte>>(StringComparer.OrdinalIgnoreCase);
        }
        using var buffer = new RentedBufferWriter<byte>(estimatedSize);
        using var file = new FileStream(_path, FileMode.Open);
        var numRead = file.Read(buffer.Buffer, 0, estimatedSize);
        buffer.Advance(numRead);
        var dict = FromSpan(buffer.WrittenSpan);
        return dict;
    }

    /// <inheritdoc />
    internal override async ValueTask<Dictionary<string, ReadOnlyMemory<byte>>> DeserializeAsync(int estimatedSize, CancellationToken cancellationToken = default) {
        if (estimatedSize is 0) {
            return new Dictionary<string, ReadOnlyMemory<byte>>(StringComparer.OrdinalIgnoreCase);
        }
        using var buffer = new RentedBufferWriter<byte>(estimatedSize);
        using var file = new FileStream(_path, FileMode.Open);
        var numRead = await file.ReadAsync(buffer.GetMemory(), cancellationToken).ConfigureAwait(false);
        buffer.Advance(numRead);
        var dict = FromSpan(buffer.WrittenSpan);
        return dict;
    }
}