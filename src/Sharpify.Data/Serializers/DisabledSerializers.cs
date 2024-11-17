using System.Collections.Concurrent;

using MemoryPack;

namespace Sharpify.Data.Serializers;

/// <summary>
/// A serializer for a database without encryption and case sensitive keys
/// </summary>
internal class DisabledSerializer : AbstractSerializer {
    internal DisabledSerializer(string path, StringEncoding encoding = StringEncoding.Utf8) : base(path, encoding) {
    }

    /// <inheritdoc />
    internal override ConcurrentDictionary<string, byte[]?> Deserialize(int estimatedSize) => new();

    /// <inheritdoc />
    internal override ValueTask<ConcurrentDictionary<string, byte[]?>> DeserializeAsync(int estimatedSize, CancellationToken cancellationToken = default) => ValueTask.FromResult(new ConcurrentDictionary<string, byte[]?>());

    /// <inheritdoc />
    internal override void Serialize(ConcurrentDictionary<string, byte[]?> dict, int estimatedSize) { }

    /// <inheritdoc />
    internal override ValueTask SerializeAsync(ConcurrentDictionary<string, byte[]?> dict, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
}

/// <summary>
/// A serializer for a database without encryption and case sensitive keys
/// </summary>
internal class DisabledIgnoreCaseSerializer : DisabledSerializer {
    internal DisabledIgnoreCaseSerializer(string path, StringEncoding encoding = StringEncoding.Utf8) : base(path, encoding) {
    }

    /// <inheritdoc />
    internal override ConcurrentDictionary<string, byte[]?> Deserialize(int estimatedSize) => new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    internal override ValueTask<ConcurrentDictionary<string, byte[]?>> DeserializeAsync(int estimatedSize, CancellationToken cancellationToken = default) => ValueTask.FromResult(new ConcurrentDictionary<string, byte[]?>(StringComparer.OrdinalIgnoreCase));
}