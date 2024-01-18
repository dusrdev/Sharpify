using System.Runtime.CompilerServices;

using MemoryPack;

namespace Sharpify.Data;

/// <summary>
/// A serializer for a database without encryption and case sensitive keys
/// </summary>
internal class IgnoreCaseSerializer : Serializer {
    internal IgnoreCaseSerializer(string path) : base(path) {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Dictionary<string, ReadOnlyMemory<byte>> FromSpan(ReadOnlySpan<byte> bin) {
        var formatter = new OrdinalIgnoreCaseStringDictionaryFormatter<ReadOnlyMemory<byte>>();
        var state = MemoryPackReaderOptionalStatePool.Rent(MemoryPackSerializerOptions.Default);
        var reader = new MemoryPackReader(bin, state);
        Dictionary<string, ReadOnlyMemory<byte>>? dict = null;
        formatter.GetFormatter().Deserialize(ref reader, ref dict!);
        return dict ?? new(StringComparer.OrdinalIgnoreCase);
    }

/// <inheritdoc />
    internal override Dictionary<string, ReadOnlyMemory<byte>> Deserialize() => FromSpan(File.ReadAllBytes(_path));

/// <inheritdoc />
    internal override async ValueTask<Dictionary<string, ReadOnlyMemory<byte>>> DeserializeAsync(CancellationToken cancellationToken = default) {
        ReadOnlyMemory<byte> bin = await File.ReadAllBytesAsync(_path, cancellationToken);
        return FromSpan(bin.Span);
    }
}