using MemoryPack;

namespace Sharpify.Data;

/// <summary>
/// A serializer for a database without encryption and case sensitive keys
/// </summary>
internal class DisabledSerializer : DatabaseSerializer {
    internal DisabledSerializer(string path, StringEncoding encoding = StringEncoding.Utf8) : base(path, encoding) {
    }

    /// <inheritdoc />
    internal override Dictionary<string, byte[]?> Deserialize(int estimatedSize) => new Dictionary<string, byte[]?>();

    /// <inheritdoc />
    internal override ValueTask<Dictionary<string, byte[]?>> DeserializeAsync(int estimatedSize, CancellationToken cancellationToken = default) => ValueTask.FromResult(new Dictionary<string, byte[]?>());

    /// <inheritdoc />
    internal override void Serialize(Dictionary<string, byte[]?> dict, int estimatedSize) { }

/// <inheritdoc />
    internal override ValueTask SerializeAsync(Dictionary<string, byte[]?> dict, int estimatedSize, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
}